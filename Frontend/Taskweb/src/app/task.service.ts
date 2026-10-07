import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../environments/environment';

export interface Task {
  id: string;
  title: string;
  description: string | null;
  categoryId: string;
  isCompleted: boolean;
}

export interface Category {
  id: string;
  name: string;
}

export type TaskForm = Omit<Task, 'id'>;

// Solo HTTP: el componente no sabe de URLs
@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  getAll(search: string) {
    return this.http.get<Task[]>(`${this.api}/tasks`, { params: { search } });
  }

  getCategories() {
    return this.http.get<Category[]>(`${this.api}/categories`);
  }

  save(task: TaskForm, id: string | null) {
    return id
      ? this.http.put(`${this.api}/tasks/${id}`, task) // editar
      : this.http.post(`${this.api}/tasks`, task); // crear
  }

  delete(id: string) {
    return this.http.delete(`${this.api}/tasks/${id}`);
  }
}
