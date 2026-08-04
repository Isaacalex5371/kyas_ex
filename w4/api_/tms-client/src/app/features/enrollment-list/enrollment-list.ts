import { Component, inject,OnInit } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment.store';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-enrollment-list',
  imports: [CommonModule],
  templateUrl: './enrollment-list.html',
  styleUrl: './enrollment-list.scss',
})
export class EnrollmentList implements OnInit {
  store = inject(EnrollmentStore);
  ngOnInit(): void {
    this.store.loadEnrollments();
  }
  onApprove(id: string) {
this.store.approveEnrollment(id);
}
}
