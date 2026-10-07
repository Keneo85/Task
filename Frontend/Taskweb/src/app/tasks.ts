import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, startWith, switchMap } from 'rxjs';
import { AuthService } from './auth';
import { Category, Task, TaskService } from './task.service';

@Component({
  selector: 'app-tasks',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tasks.html',
})
export class Tasks {
  private api = inject(TaskService);
  auth = inject(AuthService);

  tasks = signal<Task[]>([]);
  categories = signal<Category[]>([]);
  editId = signal<string | null>(null);
  error = signal('');

  // Id → nombre de categoría (computed: se recalcula solo cuando cambian las categorías)
  categoryNames = computed(() => new Map(this.categories().map((c) => [c.id, c.name])));

  search = new FormControl('', { nonNullable: true });

  form = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(100)],
    }),
    description: new FormControl('', { nonNullable: true }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    isCompleted: new FormControl(false, { nonNullable: true }),
  });

  constructor() {
    this.api.getCategories().subscribe((c) => this.categories.set(c));

    // Buscador: espera 300 ms, ignora texto repetido y cancela la búsqueda anterior
    this.search.valueChanges
      .pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((s) => this.api.getAll(s)),
        takeUntilDestroyed(),
      )
      .subscribe((t) => this.tasks.set(t));
  }

  load() {
    this.api.getAll(this.search.value).subscribe((t) => this.tasks.set(t));
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.api.save(this.form.getRawValue(), this.editId()).subscribe({
      next: () => {
        this.editId.set(null);
        this.error.set('');
        this.form.reset();
        this.load();
      },
      error: () => this.error.set('No se pudo guardar'),
    });
  }

  edit(t: Task) {
    this.editId.set(t.id);
    this.form.setValue({
      title: t.title,
      description: t.description ?? '',
      categoryId: t.categoryId,
      isCompleted: t.isCompleted,
    });
  }

  remove(t: Task) {
    if (confirm(`¿Eliminar "${t.title}"?`)) {
      this.api.delete(t.id).subscribe(() => this.load());
    }
  }
}
