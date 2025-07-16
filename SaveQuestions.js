function collectSurveyData() {
    let sectionsList = [];
    let valid = true;

    for (let section of document.querySelectorAll(".section")) {
        let sectionTitle = section.querySelector(".section-title").value;
        let sectionDescription = section.querySelector(".section-desc").value;

        if (!sectionTitle) valid = false;

        let questionsList = [];

        for (let question of section.querySelectorAll(".question")) {
            let questionText = question.querySelector(".question-text").value;
            let questionType = question.querySelector(".question-type").value;
            let mandatory = !question.querySelector(".optional").checked;

            if (!questionText || !questionType) valid = false;

            questionsList.push({
                questionNo: questionsList.length + 1,
                questionText,
                questionType,
                mandatory
            });
        }

        sectionsList.push({
            title: sectionTitle,
            description: sectionDescription,
            questions: questionsList
        });
    }

    if (valid) {
        const questionsJSON = {
            ID: document.getElementById("surveyID").value,
            SubmittedBy: document.getElementById("username").value,
            Sections: sectionsList
        };

        $.ajax({
            type: "POST",
            url: "Questions.aspx/UpdateQuestions",
            data: JSON.stringify({ info: questionsJSON }),
            contentType: "application/json; charset=utf-8",
            success: function (res) { console.log("Saved:", res); },
            error: function (err) { console.error("Error:", err); }
        });
    } else {
        alert("Missing required fields.");
    }
}
