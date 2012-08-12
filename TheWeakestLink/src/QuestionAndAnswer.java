import java.util.ArrayList;

/**
 * ADT to store questions with options and answer
 * @author Benjamin
 */
public class QuestionAndAnswer {
	String question;
	ArrayList<String> options;
	int answer;
	/**
	 * Constructor for ADT
	 * @param question Question to ask
	 * @param options List of answers to choose from
	 * @param answer model answer (number to the list of answers)
	 * @throws Exception if arguements are null or invalid
	 */
	public QuestionAndAnswer(String question, ArrayList<String> options, int answer) throws Exception
	{
		if (question == null | question.trim().equals("") | options == null | options.size() == 0 | answer >= options.size() | answer < 0)
			throw new Exception("Provide a question with non-empty list of options and a valid option");
		this.question = question;
		this.options = options;
		this.answer = answer;
	}
	@Override
	public String toString()
	{
		return question + '\n' + options + '\n' + answer + '\n';
	}
}
