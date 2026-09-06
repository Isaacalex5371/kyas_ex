import { Component, computed, signal, inject } from '@angular/core';
import { CourseCard } from '../../ui/course-card/course-card';
import { Course } from '../../models/course.model';
import { CourseService } from '../../services/course.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';

// The @Component decorator tells Angular: "This class is a visual component."
// It is metadata it describes how this class connects to the HTML template.
@Component({
  selector: 'app-student-dashboard', // The HTML tag name: <app-student-dashboard />
  standalone: true, // This component manages its own imports (no NgModule)
  imports: [CourseCard],
  templateUrl: './student-dashboard.component.html', //Points to the HTML file
  styleUrl: './student-dashboard.component.scss', //Points to the styles file
})
export class StudentDashboardComponent {
  private router = inject(Router);
  private api = inject(CourseService);
  //signal creates a reactive variable. Angular watches it.
  //When its value changes, Angular automatically updates the part of the screen that desplays it.
  studentName = signal('Liya kebede');
  earnedCredits = signal(45);
  currentPage = signal(1);

  //computed() creates a read-only signal that derives its value from other signals.
  // It recalculates automatically whenever eaernedCredit() changes no manual refresh.
  graduationStatus = computed(() =>
    this.earnedCredits() >= 120 ? 'Eligible for Graduation' : 'In Progress',
  );

  coursesResource = rxResource({
    params: () => ({
      page: this.currentPage(),
    }),
    stream: ({params}) => this.api.getAll(params.page, 9),
  });
  //A regular method. when called, it updates the earnedCreddits signal.
  //The .update() method recieves the current value (c) and returns the new value(c + 3).
  registerForClass() {
    this.earnedCredits.update((c) => c + 3);
  }

  selectedCourse = signal<Course | null>(null);

  // A sample course to display
  // availableCourses = signal<Course[]>([
  //   {
  //     id: 1,
  //     title: 'Advanced Java Services',
  //     code: 'CSE-101',
  //     capacity: 30,
  //     enrollmentCount: 10,
  //   },
  //   {
  //     id: 2,
  //     title: 'Angular UI Lab',
  //     code: 'CSE-210',
  //     capacity: 25,
  //     enrollmentCount: 25,
  //   },
  //   {
  //     id: 3,
  //     title: 'Database Design',
  //     code: 'CSE-305',
  //     capacity: 20,
  //     enrollmentCount: 18,
  //   },
  //   {
  //     id: 4,
  //     title: 'API Security Workshop',
  //     code: 'CSE-420',
  //     capacity: 40,
  //     enrollmentCount: 15,
  //   },
  // ]);

  handleEnrollment(course: Course) {
    this.selectedCourse.set(course);
    console.log('Enrollment requested for: ', course.title);
    this.router.navigate(['/enroll'],
      {
        queryParams: {
          courseId: course.id,
        }
      }
    );
  }

  nextPage() {
    const meta = this.coursesResource.value()?.meta;

    if(meta?.hasNext){
      this.currentPage.update(p => p + 1);
    }
  }

  previousPage(){
    const meta = this.coursesResource.value()?.meta;

    if(meta?.hasPrevious){
      this.currentPage.update(p => p - 1);
    }
  }
}