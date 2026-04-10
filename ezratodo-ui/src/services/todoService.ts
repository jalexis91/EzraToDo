import axios, { AxiosError } from 'axios';
import { Todo, CreateTodoRequest, UpdateTodoRequest, ApiError } from '../types/todo';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface GetTodosParams {
  isCompleted?: boolean;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: string;
}

export const todoService = {
  async getTodos(params?: GetTodosParams): Promise<Todo[]> {
    try {
      const response = await apiClient.get<{ todos: Todo[] }>('/todos', { params });
      return response.data.todos;
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async getTodoById(id: number): Promise<Todo> {
    try {
      const response = await apiClient.get<Todo>(`/todos/${id}`);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async createTodo(request: CreateTodoRequest): Promise<Todo> {
    try {
      const response = await apiClient.post<Todo>('/todos', request);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async updateTodo(id: number, request: UpdateTodoRequest): Promise<Todo> {
    try {
      const response = await apiClient.put<Todo>(`/todos/${id}`, request);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async completeTodo(id: number): Promise<void> {
    try {
      await apiClient.patch(`/todos/${id}/complete`);
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async reopenTodo(id: number): Promise<void> {
    try {
      await apiClient.patch(`/todos/${id}/reopen`);
    } catch (error) {
      throw this.handleError(error);
    }
  },

  async deleteTodo(id: number): Promise<void> {
    try {
      await apiClient.delete(`/todos/${id}`);
    } catch (error) {
      throw this.handleError(error);
    }
  },

  handleError(error: unknown): ApiError {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<ApiError>;
      if (axiosError.response?.data) {
        return axiosError.response.data;
      }
      return {
        type: 'network-error',
        title: 'Network error',
        status: axiosError.response?.status || 0,
      };
    }
    return {
      type: 'unknown-error',
      title: 'An unknown error occurred',
      status: 500,
    };
  },
};
