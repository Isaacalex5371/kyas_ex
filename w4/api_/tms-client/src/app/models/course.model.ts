/**
 * List row from the TMS API — mirrors `CourseResponseDto` on `GET /api/courses`.
 * ASP.NET Core defaults to camelCase JSON (`id`, `maxCapacity`, …).*/

export interface Course {
  id: number;
  code: string;
  title: string;
  Capacity: number;
  enrollmentCount: number;
}

/** Envelope for `GET /api/courses` — TMS API contract list shape (`PagedResponse<T>`). */
export interface PagedResponse<T> {
  data: T[];
  meta: {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasNext: number;
    hasPrevious: number;
  }
  links: {
    slef: string;
    next: string | null;
    prev: string | null;
    enroll: string | null;
  }
}

/** One link from `CourseDetailDto.Links` on `GET /api/courses/{id}`.*/
export interface CourseLink {
  href: string;
  rel: string;
  method: string;
}
/** Detail payload — mirrors `CourseDetailDto` (list rows do not include `links`). */
export interface CourseDetail extends Course {
  links: readonly CourseLink[];
}