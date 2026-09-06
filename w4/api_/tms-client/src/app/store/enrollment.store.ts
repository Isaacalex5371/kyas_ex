import { computed, inject } from '@angular/core';
import { signalStore, withComputed, withMethods, patchState, withState } from '@ngrx/signals';
import { withEntities, setAllEntities, updateEntity } from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, concatMap, tap, catchError, EMPTY, switchMap } from 'rxjs';
import { EnrollmentService } from '../services/enrollment.service';
import { Enrollment, EnrollmentStatus } from '../models/enrollment.model';
import { LiveSyncService } from '../services/live-sync.service';
export const EnrollmentStore = signalStore(
  { providedIn: 'root' },

  withState({ isLoading: false, error: null as string | null }),

  withEntities<Enrollment>(),

  withComputed((store) => ({
    PendingCount: computed(() =>
  store.entities().filter(
    e => e.status === EnrollmentStatus.Pending
  ).length
),
  })),
  withMethods((store, api = inject(EnrollmentService), sync = inject(LiveSyncService)) => ({
    listenForLiveUpdates: rxMethod<void>(
      pipe(
        tap(() => sync.connect()),
        switchMap(() => sync.events$),
        tap((event) => {
          let mappedStatus: EnrollmentStatus;
          if (event.status === 'Approved') {
            mappedStatus = EnrollmentStatus.Approved;
          } else if (event.status === 'Rejected') {
            mappedStatus = EnrollmentStatus.Rejected;
          } else {
            mappedStatus = EnrollmentStatus.Pending;
          }
          patchState(store, updateEntity({ id: event.id, changes: { status: mappedStatus } }));
        }),
      ),
    ),
     loadEnrolments: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { isLoading: true, error: null })),
        concatMap(() =>
          api.getEnrollments().pipe(
            tap((response) =>
              patchState(store, setAllEntities(response.items), { isLoading: false }),
            ),
            catchError((err) => {
              patchState(store, { isLoading: false, error: err.message });
              return EMPTY; 
            }),
          ),
        ),
      ),
    ),
     approveEnrollment: rxMethod<{ id: number; status: EnrollmentStatus }>(
      pipe(
        concatMap(({ id, status }) =>
          api.approve(id, status).pipe(
            tap((updatedEnrollment) => {
              patchState(
                store,
                updateEntity({
                  id: updatedEnrollment.id,
                  changes: updatedEnrollment,
                }),
              );
            }),
            catchError((err) => {
              patchState(store, {
                error: err.message,
              });
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);