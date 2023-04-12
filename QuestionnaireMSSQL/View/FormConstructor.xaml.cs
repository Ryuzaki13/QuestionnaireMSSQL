using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuestionnaireMSSQL.View {
	public partial class FormConstructor : Page {
		private QuestionnaireConnection connection;

		private bool lastSelectedTypeHaveVariants = false;

		public ObservableCollection<Form> Forms { get; set; }
		public Form SelectedForm { get; set; }

		public ObservableCollection<Question> Questions { get; set; }
		public Question SelectedQuestion { get; set; }

		public ObservableCollection<string> Variants { get; set; } = new ObservableCollection<string>();
		public QuestionType SelectedQuestionType { get; set; }

		public FormConstructor(QuestionnaireConnection connection) {
			InitializeComponent();
			this.connection = connection;

			Forms = new ObservableCollection<Form>(connection.Forms.ToList());

			lvForms.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = Forms });
			lvQuestionType.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() {
				Source = connection.QuestionTypes.ToList(),
			});

			DataContext = this;
		}

		private void applyQuestionFilter() {
			var view = CollectionViewSource.GetDefaultView(lvQuestions.ItemsSource);
			if (view == null) { return; }

			if (SelectedForm == null) {
				view.Filter = element => { return false; };
				return;
			}

			view.Filter = element => {
				Question question = element as Question;
				if (question == null) {
					return false;
				}

				return question.Form == SelectedForm.ID;
			};
		}

		private void changeForm(object sender, SelectionChangedEventArgs e) {
			if (Questions == null) {
				Questions= new ObservableCollection<Question>(connection.Questions.ToList());
				lvQuestions.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = Questions });
			}
			applyQuestionFilter();
			spQuestion.IsEnabled = SelectedForm != null;
		}

		private void changeQuestionType(object sender, SelectionChangedEventArgs e) {
			if (isHaveVariants()) {
				spQuestionVariants.IsEnabled = true;

				if (!lastSelectedTypeHaveVariants) {
					Variants.Clear();
					tbVariantText.Clear();
				}

				lastSelectedTypeHaveVariants = true;
			} else {
				spQuestionVariants.IsEnabled = false;

				if (lastSelectedTypeHaveVariants) {
					Variants.Clear();
					tbVariantText.Clear();
				}

				lastSelectedTypeHaveVariants = false;
			}
		}

		private bool isHaveVariants() {
			if (SelectedQuestionType == null)
				return false;
			return SelectedQuestionType.ID > 2;
		}

		private void addVariant(object sender, RoutedEventArgs e) {
			string variantText = tbVariantText.Text.Trim();
			if (variantText.Length > 0) {
				Variants.Add(variantText);
			}
			tbVariantText.Clear();
		}

		private void delVariant(object sender, RoutedEventArgs e) {
			if (lvVariants.SelectedItem == null)
				return;

			Variants.Remove((string)lvVariants.SelectedItem);
		}

		private void addQuestion(object sender, RoutedEventArgs e) {
			string questionText = tbQuestionText.Text.Trim();
			if (questionText.Length == 0) {
				MessageBox.Show("Текст вопроса не введен");
				return;
			}

			if (SelectedQuestionType == null) {
				MessageBox.Show("Необходимо выбрать тип вопроса");
				return;
			}

			if (SelectedForm == null) {
				MessageBox.Show("Необходимо выбрать анкету");
				return;
			}

			Question question = new Question();

			if (isHaveVariants()) {
				if (Variants.Count < 2) {
					MessageBox.Show("Необходимо добавить минимум 2 варианта ответа");
					return;
				}
				question.Variants = JsonSerializer.Serialize(Variants);
			}

			question.Text = questionText;
			question.QuestionType = SelectedQuestionType;
			question.Form = SelectedForm.ID;
			question.Form1 = SelectedForm;

			connection.Questions.Add(question);
			connection.SaveChanges();
			Questions.Add(question);

			Variants.Clear();

			tbVariantText.Clear();
			tbQuestionText.Clear();
			lvQuestionType.SelectedIndex = -1;
		}

		private void createQuestionnaire(object sender, RoutedEventArgs e) {
			string formName = tbQuestionnaireName.Text.Trim();
			if (formName.Length == 0) {
				MessageBox.Show("Название анкеты не введено");
				return;
			}
			Form form = new Form();
			form.Creator = 1;
			form.Name = formName;

			connection.Forms.Add(form);
			connection.SaveChanges();

			Forms.Add(form);

			lvForms.SelectedItem = form;

			tbQuestionnaireName.Clear();
		}
	}
}
