protected void GetForm()
{
    DataTable results = ExecuteSql($"SELECT * FROM Surveys s LEFT JOIN Occurrences o ON o.SurveyID = s.SurveyID WHERE o.OccurrenceID = '{hdnOccurrenceID.Value}'");
    litGreeting.Text = results.Rows[0]["HeaderText"].ToString();
    lblTitle.Text = results.Rows[0]["OccurrenceName"].ToString();
    hdnSurveyType.Value = results.Rows[0]["SurveyType"].ToString();

    if (GetAudience())
    {
        rptSections.DataSource = ExecuteSql($"SELECT * FROM Sections WHERE SurveyID = '{hdnSurveyID.Value}' ORDER BY SectionID");
        rptSections.DataBind();
    }
}

protected bool GetAudience()
{
    string query = $"SELECT SurveyType FROM Surveys s LEFT JOIN Occurrences o ON o.SurveyID = s.SurveyID WHERE o.OccurrenceID = '{hdnOccurrenceID.Value}'";
    DataTable results = ExecuteSql(query);

    hdnAudienceID.Value = Request.QueryString["A"] ?? "";

    if (new List<string> { "PPOC", "External", "Interview" }.Contains(results.Rows[0][0].ToString()) || hdnConnection.Value == "PP") return true;

    query = $@"SELECT * FROM Audience a 
                        LEFT JOIN Responses r 
                            ON r.OccurrenceID = a.OccurrenceID 
                            AND r.AudienceID = a.AudienceID 
                        WHERE a.Active = 'Y' AND a.OccurrenceID = '{hdnOccurrenceID.Value}' AND (a.PersonCode = '{hdnUsername.Value}' OR a.StaffCode = '{hdnUsername.Value}') {(hdnAudienceID.Value == "" ? "" : $"AND a.AudienceID = '{hdnAudienceID.Value}'")}";
    results = ExecuteSql(query);

    if (results.Rows.Count == 0)
    {
        divIntro.Visible = false;
        divInfo.Visible = true;
        lblInfo.Text = "<b>You are not due to complete this survey!</b> <br><br>Please go back to the Hub and select a survey that has been assigned to you.";
        divSubmit.Visible = false;
        return false;
    }

    if (results.Rows[0]["ResponseID"].ToString().Trim() != "")
    {
        divIntro.Visible = false;
        divInfo.Visible = true;
        lblInfo.Text = "You have already completed this survey. Thank you!";
        divSubmit.Visible = false;
        return false;
    }

    if (hdnAudienceID.Value == "") hdnAudienceID.Value = results.Rows[0]["AudienceID"].ToString();

    return true;
}

protected void rptSections_ItemDataBound(object sender, RepeaterItemEventArgs e)
{
    Label lblSID = (Label)e.Item.FindControl("lblSID");
    string sectionID = lblSID.Text.Trim();

    string query = $"SELECT * FROM Questions WHERE SectionID = '{sectionID}' ORDER BY CAST(SUBSTRING(QuestionNo, 1, PATINDEX('%[^0-9]%', QuestionNo + 'x') - 1) AS INT), SUBSTRING(QuestionNo, PATINDEX('%[^0-9]%', QuestionNo + 'x'), LEN(QuestionNo));";
    DataTable results = ExecuteSql(query);
    Repeater rptQuestions = (Repeater)e.Item.FindControl("rptQuestions");
    rptQuestions.DataSource = results;
    rptQuestions.DataBind();
}

protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
{
    DataRowView dataRowView = (DataRowView)e.Item.DataItem;
    string sectionID = dataRowView.Row["SectionID"].ToString();
    string questionID = dataRowView.Row["QuestionID"].ToString();
    string questionNo = dataRowView.Row["QuestionNo"].ToString();
    string questionType = dataRowView.Row["QuestionType"].ToString();
    string subType = dataRowView.Row["QuestionSubType"].ToString();
    string responseType = dataRowView.Row["ResponseType"].ToString();
    string conditional = dataRowView.Row["Conditional"].ToString();
    string multipleChoice = dataRowView.Row["MultipleChoice"].ToString();
    int columnCount = GetColumnCount(questionType, subType);
    List<string> headerNames = GetHeaderNames(questionType, subType);

    PlaceHolder phHead = (PlaceHolder)e.Item.FindControl("phHead");

    TableHeaderRow headerRow = new TableHeaderRow();
    for (int i = 0; i < columnCount; i++)
    {
        TableCell headerCell = new TableCell
        {
            Text = headerNames[i],
            CssClass = i == 0 ? "w-200px" : "",
            HorizontalAlign = HorizontalAlign.Center
        };
        headerCell.ControlStyle.Font.Bold = true;
        headerRow.Cells.Add(headerCell);
    }

    phHead.Controls.Add(headerRow);

    string query = "";
    if (responseType == "Single")
    {
        query = $"SELECT '{sectionID}' AS SectionID, '{questionID}' AS QuestionID, '{questionNo}' AS QuestionNo, '{questionType}' AS QuestionType, '{subType}' AS QuestionSubType, 'Response:' AS RespondTo, '{Conditional}' AS Conditional, '{multipleChoice}' AS MultipleChoice";
        headerRow.Cells[0].Visible = false;
    }
    else // responseType == Custom 
    {
        query = $"SELECT '{sectionID}' AS SectionID, '{questionID}' AS QuestionID, '{questionNo}' AS QuestionNo, '{questionType}' AS QuestionType, '{subType}' AS QuestionSubType, SPEC_VALUE AS RespondTo, '{Conditional}' AS Conditional, '{multipleChoice}' AS MultipleChoice FROM QuestionOptions WHERE SectionID = '{sectionID}' AND QuestionNo = '{questionNo}' AND SpecType = 'RespondTo'";
    }
    Repeater rptInner = (Repeater)e.Item.FindControl("rptInner");
    DataTable dtInner = ExecuteSql(query);
    rptInner.DataSource = dtInner;
    rptInner.DataBind();
}

protected int GetColumnCount(string questionType, string subType)
{
    switch (questionType)
    {
        case "W":
        case "D":
        case "DD":
            return 2;
        case "MC":
            switch (subType)
            {
                case "DA":
                case "OF":
                    return 5;
                case "DAN":
                case "OFN":
                    return 6;
                case "OT":
                    return 11;
                case "OTN":
                    return 12;
                default:
                    return 1;
            }
        default:
            return 1;
    }
}

protected List<string> GetHeaderNames(string questionType, string subType)
{
    switch (questionType)
    {
        case "W":
        case "D":
        case "DD":
            return new List<string> { "", "" };
        case "MC":
            switch (subType)
            {
                case "DA":
                    return new List<string> { "", "Completely Agree", "Agree", "Disagree", "Completely Disagree" };
                case "DAN":
                    return new List<string> { "", "Completely Agree", "Agree", "Disagree", "Completely Disagree", "N/A" };
                case "OT":
                    return new List<string> { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
                case "OTN":
                    return new List<string> { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "N/A" };
                case "OF":
                    return new List<string> { "", "1", "2", "3", "4" };
                case "OFN":
                    return new List<string> { "", "1", "2", "3", "4", "N/A" };
                default:
                    return new List<string> { "" };
            }
        default:
            return new List<string> { "" };
    }
}

protected void rptInner_ItemDataBound(object sender, RepeaterItemEventArgs e)
{
    DataRowView dataRowView = (DataRowView)e.Item.DataItem;
    string sectionID = dataRowView.Row["SectionID"].ToString();
    string questionID = dataRowView.Row["QuestionID"].ToString();
    string questionNo = dataRowView.Row["QuestionNo"].ToString();
    string questionType = dataRowView.Row["QuestionType"].ToString();
    string subType = dataRowView.Row["QuestionSubType"].ToString();
    string respondTo = dataRowView.Row["RespondTo"].ToString();
    string Conditional = dataRowView.Row["Conditional"].ToString();
    string multipleChoice = dataRowView.Row["MultipleChoice"].ToString();
    int columnCount = GetColumnCount(questionType, subType);

    PlaceHolder phBody = (PlaceHolder)e.Item.FindControl("phBody");

    TableRow bodyRow = new TableRow();
    for (int i = 0; i < columnCount; i++)
    {
        TableCell bodyCell = new TableCell
        {
            CssClass = i == 0 ? "w-200px" : "",
            HorizontalAlign = HorizontalAlign.Center,
            VerticalAlign = VerticalAlign.Middle
        };

        if (i == 0)
        {
            bodyCell.Visible = respondTo != "Response:";
            bodyCell.Text = respondTo;
        }
        else
        {
            if (questionType == "W")
            {
                TextBox txt = new TextBox
                {
                    ID = $"txt_{questionNo}_{e.Item.ItemIndex}",
                    CssClass = "form-control",
                    TextMode = subType == "M" ? TextBoxMode.MultiLine : TextBoxMode.SingleLine,
                    Rows = 2
                };
                bodyCell.Controls.Add(txt);
            }
            else if (questionType == "D")
            {
                TextBox txt = new TextBox
                {
                    ID = $"txt_{questionNo}_{e.Item.ItemIndex}",
                    CssClass = "form-control",
                    TextMode = TextBoxMode.Date,
                    Width = Unit.Pixel(200)
                };
                txt.Style.Add("float", "left");
                bodyCell.Controls.Add(txt);
            }
            else if (questionType == "DD")
            {
                DropDownList ddl = new DropDownList
                {
                    ID = $"ddl_{questionNo}_{e.Item.ItemIndex}",
                    CssClass = "form-select"
                };

                // get values
                string query;

                if (subType == "YN")
                {
                    ddl.Items.Add("Yes");
                    ddl.Items.Add("No");

                    if (Conditional == "Y") ddl.Attributes["onchange"] = "ddlConditionalOnChange(this)";
                }
                else
                {
                    ddl.DataSource = ExecuteSql($"SELECT SPEC_VALUE FROM Questions_SPEC WHERE SectionID = '{sectionID}' AND QuestionNo = '{questionNo}' AND SPEC_TYPE = 'OPTION' ORDER BY SPEC_ID");
                    ddl.DataValueField = "SPEC_VALUE";
                    ddl.DataTextField = "SPEC_VALUE";
                    ddl.DataBind();
                }
                if (multipleChoice != "Y") ddl.Items.Insert(0, new ListItem("Please select...", "0"));

                // set attributes 
                if (ddl.Items.Count > 9 || multipleChoice == "Y") ddl.Attributes.Add("data-control", "select2");
                if (multipleChoice == "Y")
                {
                    ddl.Attributes["onchange"] = "ddlMultipleChoiceOnChange(this)";
                    ddl.Attributes.Add("multiple", "multiple");
                    ddl.Attributes.Add("placeholder", "Please select...");
                }

                // set prepop answer
                if (hdnConnection.Value == "EBS")
                {
                    query = $"SELECT * FROM Prepopulated WHERE QuestionID = '{questionID}' AND AccountName = '{hdnUsername.Value}'";
                    DataTable results = ExecuteSql(query);
                    if (results.Rows.Count > 0 && ddl.Items.Cast<ListItem>().Select(item => item.Value).ToList().Contains(results.Rows[0]["Response"].ToString())) ddl.SelectedValue = results.Rows[0]["Response"].ToString();
                }

                bodyCell.Controls.Add(ddl);

                if (multipleChoice == "Y")
                {
                    HiddenField hdn = new HiddenField
                    {
                        ID = $"hdn_{questionNo}_{e.Item.ItemIndex}"
                    };
                    bodyCell.Controls.Add(hdn);
                }
            }
            else if (questionType == "MC")
            {
                RadioButton rb = new RadioButton
                {
                    ID = $"rb_{questionNo}_{e.Item.ItemIndex}_{i}",
                    GroupName = questionNo
                };
                bodyCell.Controls.Add(rb);
            }
        }

        bodyRow.Cells.Add(bodyCell);
    }

    phBody.Controls.Add(bodyRow);
}