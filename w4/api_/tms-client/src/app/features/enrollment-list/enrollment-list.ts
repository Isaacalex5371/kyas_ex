import { Component, effect, inject, OnInit, viewChild } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment.store';
import { Enrollment, EnrollmentStatus } from '../../models/enrollment.model';
import {MatTableModule, MatTableDataSource} from '@angular/material/table';
import {MatPaginatorModule, MatPaginator} from '@angular/material/paginator';
import {MatSortModule, MatSort} from '@angular/material/sort';
import { AuthService } from '../../services/auth.service';
@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [MatTableModule, MatPaginatorModule, MatSortModule],
  templateUrl: './enrollment-list.html',
  styleUrl: './enrollment-list.scss',
})
export class EnrollmentListComponent {
  store = inject(EnrollmentStore);
  auth = inject(AuthService);
  displayedColumns = ['studentName', 'courseName', 'status', 'actions'];
  //MatTableDataSource bridges our store data into Material's rendering pipeline
  dataSource = new MatTableDataSource<Enrollment>();

  //viewChild.required() is Angular 22's signal-based replacement for @ViewChild.
  //Unlike the Legacy decorator, these are signals - they update reactively when Anfular resolves the template queries.  No ngAfrerViewInit lifecycle hood needed.
  readonly paginator = viewChild.required(MatPaginator);
  readonly sort = viewChild.required(MatSort);

  constructor(){
    //Effect 1: Push store entities into the Material data source whenever they change.
    //Every time the store updates (Approve, Load, rollback), this effect fires and the table re-renders with fresh data.
    effect(() => {
      this.dataSource.data = this.store.entities();
    })

    //Effect 2: Wire paginator and sort controls once Angular resolves the view queries.
    //Because Viewchild returns a signal, this effect re-runs when the paginator or sor directives become available - no manual lifecycle hook needed.
    effect(() => {
      this.dataSource.paginator = this.paginator();
      this.dataSource.sort = this.sort();
    });

    //Load enrollments on component creation
    this.store.loadEnrolments();
  }

  enrollmentStatus = EnrollmentStatus;

  onApprove(id: number){
    this.store.approveEnrollment({id, status: EnrollmentStatus.Approved});
  }

  onReject(id: number){
    this.store.approveEnrollment({id, status: EnrollmentStatus.Rejected})
  }
}