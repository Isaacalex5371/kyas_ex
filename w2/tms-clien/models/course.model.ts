// import { Temporal } from '@js-temporal/polyfill';

import { Temporal } from "@js-temporal/polyfill";
export interface Course{
    readonly id:string;
    title:string;
    capacity:Number;
    startDate?:Temporal.PlainDate;
}

export type CourseStatus =
| { status: "DRAFT"; createdBy: string; createdAt: Temporal.Instant }
| { status: "PUBLISHED"; publishedAt: Temporal.Instant; syllabus: string }
| {
status: "ACTIVE";
enrolledCount: number;
startDate: Temporal.PlainDate;
}
| {
status: "ARCHIVED";
archivedAt: Temporal.Instant;
finalEnrollmentCount: number;
}
| { status: "CANCELLED"; reason: string; cancelledAt: Temporal.Instant };
export function describeCourse(course: CourseStatus): string {
  switch (course.status) {
    case "DRAFT":
      return `Draft created by ${course.createdBy}`;

    case "PUBLISHED":
      return `Published with syllabus: ${course.syllabus}`;

    case "ACTIVE":
      return `Active with ${course.enrolledCount} enrolled students`;

    case "ARCHIVED":
      return `Archived with final enrollment count of ${course.finalEnrollmentCount}`;

    case "CANCELLED":
      return `Cancelled: ${course.reason}`;

    default: {
      const _check: never = course;
      throw new Error(`Unhandled status: ${JSON.stringify(_check)}`);
    }
  }
}

