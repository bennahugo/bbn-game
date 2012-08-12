import java.util.ArrayList;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.DocumentBuilder;
import org.w3c.dom.Document;
import org.w3c.dom.NodeList;
import org.w3c.dom.Node;
import org.w3c.dom.Element;
import java.io.File;
/**
 * Class that generates questions in a cyclical fashion.
 * @author Benjamin
 */
public class QuestionGenerator {
	int currentQuestionNumber = 0;
	ArrayList<QuestionAndAnswer> questionList;
	/**
	 * Constructor for QuestionGenerator
	 * reference: http://www.mkyong.com/java/how-to-read-xml-file-in-java-dom-parser/
	 * @param questionsDBLocation File location of file containing questions and answers
	 * @throws Exception if input XML file could not be read
	 */
	public QuestionGenerator(String questionsDBLocation) throws Exception
	{
		//instantiate storage for questions and answers
		questionList = new ArrayList<QuestionAndAnswer>();
		
		//read xml file
		File fXmlFile = new File(questionsDBLocation);
		DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
		DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
		Document doc = dBuilder.parse(fXmlFile);
		doc.getDocumentElement().normalize();
		NodeList nList = doc.getElementsByTagName("entry");
		for (int i = 0; i < nList.getLength(); ++i) //foreach <entry> in <questionlist>
		{
			Node nNode = nList.item(i);
			if (nNode.getNodeType() == Node.ELEMENT_NODE) {
				Element eElement = (Element) nNode;
				String question = getTagValues("question",eElement).get(0);
				
				ArrayList<String> options = getTagValues("option",eElement);
				int answerindex = Integer.parseInt(getTagValues("answerindex",eElement).get(0));
				questionList.add(new QuestionAndAnswer(question, options, answerindex));
			}
		}
	}
	/**
	 * Reads a list of child nodes from the current element
	 * reference: http://www.mkyong.com/java/how-to-read-xml-file-in-java-dom-parser/
	 * @param sTag tag name of the child elements in question 
	 * @param eElement handle to the parent containing the elements
	 * @return list of child node values
	 */
	private static ArrayList<String> getTagValues(String sTag, Element eElement) {
		ArrayList<String> results = new ArrayList<String>();
		NodeList elementList = eElement.getElementsByTagName(sTag);
		for (int e = 0; e < elementList.getLength(); ++e)
		{
			NodeList nlList = eElement.getElementsByTagName(sTag).item(e).getChildNodes();
			for (int i = 0; i < nlList.getLength(); ++i)
				results.add(nlList.item(i).getNodeValue());
		}
		return results;
	}
	/**
	 * Gets the next question
	 * @return question string
	 */
	public String getQuestion()
	{
		return questionList.get(currentQuestionNumber).question;
	}
	/**
	 * Checks if the player provided the correct answer. Increments the current question pointer to the next question
	 * @param chosenOption as int
	 * @return true if answer is correct
	 */
	public boolean checkAnswer(int chosenOption)
	{
		boolean result = chosenOption >= 0 && chosenOption < questionList.size() ? 
				questionList.get(this.currentQuestionNumber).answer == chosenOption : false;
		currentQuestionNumber = currentQuestionNumber >= questionList.size() - 1 ? 0 : currentQuestionNumber + 1;
		return result;
	}
}
