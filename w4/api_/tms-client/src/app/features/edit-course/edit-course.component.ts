import { Component, inject, signal } from '@angular/core';
import { CourseService } from '../../services/course.service';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-edit-course',
 standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-course.component.html',
  styleUrl: './edit-course.component.scss',
})
export class EditCourseComponent {
  private courseservice = inject(CourseService);
  private router = inject(Router);

  courseTitle = signal('');
  Id= signal(null);
  isLoading = signal(false);
  successMessage = signal<string|null>(null);
  errorMessage= signal<string|null>(null);

  onSubmit(): void {
    const courseId = Number(this.Id());
    if(!this.courseTitle().trim()){
      this.errorMessage.set('Please enter a valid course title.');
      return;
    }
    this.isLoading.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.courseservice.updateCourse(courseId,{title:this.courseTitle()}).subscribe({

    })
  }
}
