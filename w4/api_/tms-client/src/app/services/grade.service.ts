import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';

export interface GradePlayload{
    studentId:number;
    courseId:number;
    score:number;
}

@Service()
export class GradeService {
private http= inject(HttpClient);
postGrade(playload:GradePlayload):Observable<{id:string;success:boolean}>{
    return this.http.post<{id:string;success:boolean}>('/api/grades',playload);
}
}
