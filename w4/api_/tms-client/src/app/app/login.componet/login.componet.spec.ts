import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginComponet } from './login.componet';

describe('LoginComponet', () => {
  let component: LoginComponet;
  let fixture: ComponentFixture<LoginComponet>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponet],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponet);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
