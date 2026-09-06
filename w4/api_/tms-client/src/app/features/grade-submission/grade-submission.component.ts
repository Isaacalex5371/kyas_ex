import { Component, inject } from '@angular/core';
import { GradePlayload,GradeService } from '../../services/grade.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { exhaustMap, Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';


@Component({
  selector: 'app-grade-submission',
  imports: [
  MatCardModule,
  ReactiveFormsModule,
  MatFormFieldModule,
  MatInputModule,
  MatButtonModule,
  MatProgressSpinnerModule,
],
  templateUrl: './grade-submission.component.html',
  styleUrl: './grade-submission.component.scss',
})
export class GradeSubmissionComponent {
  private api = inject(GradeService);
  private fb = inject(FormBuilder);
  // Reactive Form definition with initial model values and validators
  gradeForm = this.fb.group({
    studentId: ["", [Validators.required, Validators.min(1)]],
    courseId: ["", [Validators.required, Validators.min(1)]],
    score: ["", [Validators.required, Validators.min(0), Validators.max(100)]],
  });
  isSubmitting = false;
  submissionStatus = '';
  // A Subject is a manual event stream — template clicks push payloads into it
  private submitClick$ = new Subject<GradePlayload>();
  constructor() {
    this.submitClick$
      .pipe(
        // exhaustMap: while the inner HTTP observable is active,
        // ALL new emissions from submitClick$ are silently dropped.
        exhaustMap((payload) => {
          this.isSubmitting = true;
          this.submissionStatus = 'Submitting grade to server...';
          return this.api.postGrade(payload);
        }),
        // takeUntilDestroyed: automatically unsubscribes when Angular destroys this component, preventing memory leaks.
        // Placed inside constructor to inherit the active injectioncontext.
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (result) => {
          this.isSubmitting = false;
          this.submissionStatus = `Grade saved successfully! Record ID:${result.id}`;
        },
        error: (err) => {
          this.isSubmitting = false;
          this.submissionStatus = `Submission failed: ${err.message || 'Server error'}`;
        },
      });
  }
  // The template form submit handler pushes valid values into the protected stream
  onSubmit() {
    if (this.gradeForm.valid) {
      const rawValue = this.gradeForm.getRawValue();
      this.submitClick$.next({
        studentId: Number(rawValue.studentId),
        courseId: Number(rawValue.courseId),
        score: Number(rawValue.score),
      });
    }
  }
}