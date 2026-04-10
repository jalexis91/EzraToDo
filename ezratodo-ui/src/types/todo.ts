export interface Todo {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTodoRequest {
  title: string;
  description?: string;
  dueDate?: string;
}

export interface UpdateTodoRequest {
  title?: string;
  description?: string;
  dueDate?: string;
  isCompleted?: boolean;
}

export interface ApiError {
  type: string;
  title: string;
  status: number;
  errors?: Record<string, string[]>;
}
