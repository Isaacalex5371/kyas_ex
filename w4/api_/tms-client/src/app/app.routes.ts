import { Routes } from '@angular/router';
import { EnrollmentListComponent } from './features/enrollment-list/enrollment-list';
import { roleGuard } from './guards/role-guard';
import { StudentDashboardComponent } from './features/student-dashboard/student-dashboard.component';
import { AdminCourseList } from './admin-course-list/admin-course-list';

export const routes: Routes = [
  {
    //
    path: 'dashboard',
    loadComponent: () =>
      import('./features/student-dashboard/student-dashboard.component').then(
        (m) => m.StudentDashboardComponent,
      ),
  },
  {
    //
    path:'i-dashboard'
    ,
    loadComponent:()=>
      import('./features/instructor-dashboard/instructor-dashboard').then (
    (m)=> m.InstructorDashboard),
    canActivate: [roleGuard('Instructor')],

  }
  ,
  
  {
    //
    path: 'courses/:id',
    loadComponent: () =>
      import('./features/course-detail/course-detail').then((m) => m.CourseDetail),
  },{
path: 'enroll',
loadComponent: () => import('./features/enrollment-form/enrollment-form')
.then(m => m.EnrollmentForm)
},
{
  //
  path:'queue',
 
  loadComponent:()=>
    import('./features/enrollment-list/enrollment-list').then((m)=>m.EnrollmentListComponent),
   canActivate: [roleGuard('admin')],
},
{
  //
path: 'grade-submission',
loadComponent: () =>
import('./features/grade-submission/grade-submission.component')
.then(m => m.GradeSubmissionComponent)
},
{
  //
path: 'login',
loadComponent: () =>
import('./features/login/login.component')
.then(m => m.LoginComponent)
},
{
path: 'admin/courses',
component: AdminCourseList,
canActivate: [roleGuard('admin')]
},
{path:'unauthorized',
  loadComponent:()=>import('./features/unauthorized/unauthorized.component').then(
    (m)=>m.UnauthorizedComponent
  )
},
{
  path:'update-course',
  canActivate:[roleGuard('Instructor')],
  loadChildren:()=>import('./features/edit-course/edit-course.component').then((m)=>m.EditCourseComponent,),
},
{
  path:'enroll',
  loadComponent:()=>import('./features/enrollment-form/enrollment-form').then((m)=>m.EnrollmentForm),

},
{
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },

];
