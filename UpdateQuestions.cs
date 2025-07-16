[WebMethod]
public static string UpdateQuestions(SurveyData info)
{
    try
    {
        // remove old question options and section data for this survey
        ExecuteSql($"DELETE FROM QuestionOptions WHERE SurveyID = '{info.ID}'; DELETE FROM SurveySections WHERE SurveyID = '{info.ID}'");

        foreach (Section section in info.Sections)
        {
            // insert new section and returns its SectionID
            string sectionId = InsertAndReturnId($"INSERT INTO SurveySections (SurveyID, Title, Description) VALUES ('{info.ID}', '{section.Title}', '{section.Description}')");

            foreach (Question q in section.Questions)
            {
                if (q.QuestionID == -1)
                {
                    string insertQ = $@"INSERT INTO SurveyQuestions (SectionID, QuestionNo, Title, Type, SubType, ResponseType, Mandatory)
                                     VALUES ('{sectionId}', '{q.QuestionNo}', '{q.QuestionText}', '{q.QuestionType}', '{q.QuestionSubType}', '{q.ResponseType}', '{q.Mandatory}');";
                    ExecuteSql(insertQ);
                }
                else
                {
                    string updateQ = $@"UPDATE SurveyQuestions SET
                                     Title = '{q.QuestionText}', Type = '{q.QuestionType}', SubType = '{q.QuestionSubType}', ResponseType = '{q.ResponseType}', 
                                     Mandatory = '{q.Mandatory}' WHERE QuestionID = '{q.QuestionID}';";
                    ExecuteSql(updateQ);
                }

                // insert spec values if there's any custom values
                foreach (string value in q.QuestionSubTypeValues)
                    ExecuteSql($"INSERT INTO QuestionOptions (SectionID, QuestionNo, Type, Value) VALUES ('{sectionId}', '{q.QuestionNo}', 'OPTION', '{value}')");

                foreach (string value in q.ResponseTypeValues)
                    ExecuteSql($"INSERT INTO QuestionOptions (SectionID, QuestionNo, Type, Value) VALUES ('{sectionId}', '{q.QuestionNo}', 'RESPOND_TO', '{value}')");
            }
        }

        // remove any old questions that were deleted on the frontend
        ExecuteSql($@"
            DELETE q FROM SurveyQuestions q
            LEFT JOIN SurveySections s ON s.SectionID = q.SectionID
            WHERE s.SectionID IS NULL
        ");

        return "true";
    }
    catch (Exception ex)
    {
        // logs error into internal errors table for debugging
        LogError("UpdateQuestions", ex, info.ID, info.SubmittedBy);
        return $"false|{ex.Message}";
    }
}


public class SurveyData
{
    public string ID { get; set; }
    public string SubmittedBy { get; set; }
    public List<Section> Sections { get; set; }
}

public class Section
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<Question> Questions { get; set; }
}

public class Question
{
    public int QuestionID { get; set; }
    public string QuestionNo { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public string QuestionSubType { get; set; }
    public List<string> QuestionSubTypeValues { get; set; }
    public string ResponseType { get; set; }
    public List<string> ResponseTypeValues { get; set; }
    public string Mandatory { get; set; }
}
