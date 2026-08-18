import { inject } from '@angular/core';
import { signalStore, withMethods, patchState, withState } from '@ngrx/signals';
import { withEntities, setAllEntities, removeEntity } from '@ngrx/signals/entities';
import { catchError, EMPTY } from 'rxjs';
import { CourseService } from '../services/course.service';
import { Course } from '../models/course.model';

export const CourseStore = signalStore(
  { providedIn: 'root' },
  withState({ isLoading: false, error: null as string | null }),
  withEntities<Course>(),
  withMethods((store, svc = inject(CourseService)) => ({
    deleteCourse(id: number) {
      // 1. CRITICAL: Take snapshot of current entities BEFORE mutating local state [3, 4]
      const previousSnapshot = store.entities();

      // 2. Instant visual feedback — remove entity immediately from local UI [3, 4]
      patchState(store, removeEntity(id));

    
      svc.deleteCourse(id).pipe(
        catchError((err) => {
          // 4. Server rejected request (409 Conflict) — restore previous snapshot cleanly [4]
          patchState(store, setAllEntities(previousSnapshot));
          patchState(store, {
            error: 'Cannot delete course: active student enrollments exist.',
          });
          return EMPTY;
        })
      ).subscribe();
    }
  }))
);