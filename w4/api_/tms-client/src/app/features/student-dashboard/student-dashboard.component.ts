import { Course } from '../../models/course.model';
import { Component, signal, computed, input, output, inject } from '@angular/core';
import { CourseCard } from '../../ui/course-card/course-card';
import { Title } from '@angular/platform-browser';
import { rxResource } from '@angular/core/rxjs-interop';
import { CourseService } from '../../services/course.service';
@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CourseCard],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
})
export class StudentDashboardComponent {
  private api = inject(CourseService);
  studentName = signal('Liya Kebede');
  earnedCredits = signal(45);
   selectedCourse = signal<Course | null>(null);
  gradduationStatus = computed(() =>
    this.earnedCredits() >= 120 ? 'Eligible fro Graduation' : 'In progress',
  );
  coursesResource= rxResource({
    stream: ()=> this.api.getAll(),
  })

  registerForClass() {
    this.earnedCredits.update((c) => c + 3);
  }
 

  
  handleEnroll(course: Course) {
    this.selectedCourse.set(course);
    console.log('Enrollment requested for:', course.title);
  }
}
