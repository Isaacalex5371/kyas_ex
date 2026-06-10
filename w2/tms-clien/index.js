"use strict";
// import { Temporal } from "@js-temporal/polyfill";
Object.defineProperty(exports, "__esModule", { value: true });
const assessment_model_1 = require("./models/assessment.model");
const polyfill_1 = require("@js-temporal/polyfill");
// import { Student } from "./models/student.model";
const student = {
    id: "STU-001", name: "Hana Tadesse", enrollmentDate: polyfill_1.Temporal.Now.instant(),
};
// Try these what does the compiler say?
// student.id = "STU-999";
// console.log(student.gpa.toFixed(2));
console.log(student.gpa?.toFixed(2) ?? "Not yet graded");
const quiz = {
    id: "QUIZ-001",
    kind: "quiz", title: "SQL Basics", correctAnswers: 8, totalQuestions: 10,
};
const lab = {
    id: "LAB-001", kind: "lab", title: "REST API Project", functionalityScore: 85, codeQualityScore: 90,
};
console.log(`Quiz grade: ${(0, assessment_model_1.calculateGrade)(quiz)}%`); // 80
console.log(`Lab grade: ${(0, assessment_model_1.calculateGrade)(lab)}%`); // 87
// Verify readonly try this line and check the compiler error:
// quiz.id = "QUIZ-999";
// ERROR: Cannot assign to 'id' because it is a read-only property
