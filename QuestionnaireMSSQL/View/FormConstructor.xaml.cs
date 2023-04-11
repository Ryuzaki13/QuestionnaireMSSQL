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
		public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
		public ObservableCollection<string> Variants { get; set; } = new ObservableCollection<string>();
		public QuestionType SelectedQuestionType { get; set; }

		public FormConstructor(QuestionnaireConnection connection) {
			InitializeComponent();
			this.connection = connection;

			lbQuestionType.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() {
				Source = connection.QuestionTypes.ToList(),
			});

			DataContext = this;
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

			Questions.Add(question);
		}

		private void saveQuestionnaire(object sender, RoutedEventArgs e) {
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

			foreach (var question in Questions) {
				question.Form = form.ID;
				connection.Questions.Add(question);
			}
			connection.SaveChanges();

			Variants.Clear();
			Questions.Clear();
			tbQuestionnaireName.Clear();
			tbVariantText.Clear();
			tbQuestionText.Clear();
			lbQuestionType.SelectedIndex = -1;
		}
	}
}
