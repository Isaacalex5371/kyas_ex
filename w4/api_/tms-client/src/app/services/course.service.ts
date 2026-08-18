import { Service, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map } from "rxjs/operators";
import { environment } from "../../environments/environment.development";
import { Course, PagedResponse } from "../models/course.model";
// @Service() means Angular creates one instance of this service
// and shares it across the entire app. This is the Angular 22 shorthand replacing legacy @Injectable.
// This is similar to AddSingleton<T>() in .NET's dependency injection.
@Service()
export class CourseService {
private http = inject(HttpClient);
private readonly base = `${environment.apiUrl}/courses`;
getAll() {
return this.http
.get<PagedResponse<Course>>(this.base, {
params: { page: '1', pageSize: '50' }
})
.pipe(map(response => response.data));
}
 deleteCourse(id: number) {
    return this.http.delete(`${this.base}/${id}`);
  }
}