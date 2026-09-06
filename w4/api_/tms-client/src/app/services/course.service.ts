import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Course, CourseDetail, PagedResponse } from '../models/course.model';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface UpdateCourseDto{
  title: string;
}
@Service()
export class CourseService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/v2/courses`;

  getAll(page = 1, pageSize = 12) {
    return this.http.get<PagedResponse<Course>>(this.baseUrl, {
      params: { page: page.toString(), pageSize: pageSize.toString() },
    });
  }

  getById(id: string) {
    return this.http.get<CourseDetail>(`${this.baseUrl}/${id}`);
  }

  deleteCourse(id: number){
    return this.http.delete<void>(`${this.baseUrl}/${id}`)
  }

  updateCourse(id: number, dto: UpdateCourseDto): Observable<void>{
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }
}