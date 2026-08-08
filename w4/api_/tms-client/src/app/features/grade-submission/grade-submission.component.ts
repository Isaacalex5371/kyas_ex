import { Component, inject } from '@angular/core';
import { GradePlayload, GradeService } from '../../services/grade.service';
import { FormBuilder, Validators } from '@angular/forms';
import { exhaustMap, Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReactiveFormsModule } from '@angular/forms';
@Component({
  selector: 'app-grade-submission',
  standalone: true,
  imports: [ ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule],
  templateUrl: './grade-submission.component.html',
  styleUrl: './grade-submission.component.scss',
})
export class GradeSubmissionComponent {
  private api = inject(GradeService);
  private fb = inject(FormBuilder);
  gradeForm = this.fb.group({
    studentId: [101, [Validators.required, Validators.min(1)]],
    courseId: [302, [Validators.required, Validators.min(1)]],
    score: [88, [Validators.required, Validators.min(0), Validators.max(100)]],
  });

  isSubmitting = false;
  submissionStatus = '';
  private submitClick$ = new Subject<GradePlayload>();
  constructor() {
    this.submitClick$
      .pipe(
        exhaustMap((payload) => {
          this.isSubmitting = true;
          this.submissionStatus = 'Submitting grade to server...';
          return this.api.postGrade(payload);
        }),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (result) => {
          this.isSubmitting = false;
          this.submissionStatus = `grade saved successfully ! Record Id: ${result.id}`;
        },
        error: (err) => {
          this.isSubmitting = false;
          this.submissionStatus = `submition faild : ${err.message || 'server error'}`;
        },
      });
  }
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
