namespace QuestionnaireMSSQL.View {
	public class PageControl {
		private static QuestionnaireConnection connection;
		private static FormConstructor formConstructor;

		public static FormConstructor FormConstructor {
			get {
				if (formConstructor == null) {
					formConstructor = new FormConstructor(GetConnection());
				}
				return formConstructor;
			}
		}

		private static QuestionnaireConnection GetConnection() {
			if (connection == null) {
				connection = new QuestionnaireConnection();
			}
			return connection; 
		}
	}
}
