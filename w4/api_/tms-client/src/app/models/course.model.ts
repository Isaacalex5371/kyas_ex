export interface Course{
    id:number;
    code:string;
    title:string;
    Capacity:number;
    enrollmentCount:number;
    status?: string;
}

export interface PagedResponse<T>{
    data: T[];
    totalCount:number;
    page:number;
    pageSize:number;
    totalPages:number;
    hasPrevious:boolean;
    hasNext:boolean;
    }

export interface CourseLink{
    href:string;
    rel:string;
    method:string;
}

export interface CourseDetail extends Course{
    links: readonly CourseLink[];
}