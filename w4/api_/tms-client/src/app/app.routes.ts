import { Routes } from '@angular/router';
import { EnrollmentList } from './features/enrollment-list/enrollment-list';

export const routes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/student-dashboard/student-dashboard.component').then(
        (m) => m.StudentDashboardComponent,
      ),
  },
  {
    path:'i-dashboard'
    ,
    loadComponent:()=>
      import('./features/instructor-dashboard/instructor-dashboard').then (
    (m)=> m.InstructorDashboard)
  }
  ,
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'courses/:id',
    loadComponent: () =>
      import('./features/course-detail/course-detail').then((m) => m.CourseDetail),
  },{
path: 'enroll',
loadComponent: () => import('./features/enrollment-form/enrollment-form')
.then(m => m.EnrollmentForm)
},
{path:'queue',
  loadComponent:()=>
    import('./features/enrollment-list/enrollment-list').then((m)=>EnrollmentList)
},
{
path: 'grade-submission',
loadComponent: () =>
import('./features/grade-submission/grade-submission.component')
.then(m => m.GradeSubmissionComponent)
}
];
