export interface PagedResponsee<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
export enum EnrollmentStatus {
   Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
}
export interface Enrollment {
  id: number;
  studentId: number;
  studentName: string;
  courseId: number;
  courseName: string;
  status: EnrollmentStatus;
  enrolledAt: Date;
}