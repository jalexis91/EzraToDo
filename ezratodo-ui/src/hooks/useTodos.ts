import { useState, useEffect, useCallback } from 'react';
import { todoService, GetTodosParams } from '../services/todoService';
import { Todo, CreateTodoRequest, UpdateTodoRequest, ApiError } from '../types/todo';

interface UseTodosState {
  todos: Todo[];
  loading: boolean;
  error: ApiError | null;
}

export const useTodos = (initialParams: GetTodosParams = {}) => {
  const [state, setState] = useState<UseTodosState>({
    todos: [],
    loading: false,
    error: null,
  });
  const [params, setParams] = useState<GetTodosParams>(initialParams);

  const fetchTodos = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    try {
      const todos = await todoService.getTodos(params);
      setState(prev => ({ ...prev, todos, loading: false }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        error: error as ApiError,
        loading: false,
      }));
    }
  }, [params]);

  const createTodo = useCallback(async (request: CreateTodoRequest) => {
    try {
      const newTodo = await todoService.createTodo(request);
      setState(prev => ({
        ...prev,
        todos: [...prev.todos, newTodo],
        error: null,
      }));
      return newTodo;
    } catch (error) {
      setState(prev => ({ ...prev, error: error as ApiError }));
      throw error;
    }
  }, []);

  const updateTodo = useCallback(async (id: number, request: UpdateTodoRequest) => {
    try {
      const updated = await todoService.updateTodo(id, request);
      setState(prev => ({
        ...prev,
        todos: prev.todos.map(t => t.id === id ? updated : t),
        error: null,
      }));
      return updated;
    } catch (error) {
      setState(prev => ({ ...prev, error: error as ApiError }));
      throw error;
    }
  }, []);

  const completeTodo = useCallback(async (id: number) => {
    try {
      await todoService.completeTodo(id);
      setState(prev => ({
        ...prev,
        todos: prev.todos.map(t => t.id === id ? { ...t, isCompleted: true } : t),
        error: null,
      }));
    } catch (error) {
      setState(prev => ({ ...prev, error: error as ApiError }));
      throw error;
    }
  }, []);

  const reopenTodo = useCallback(async (id: number) => {
    try {
      await todoService.reopenTodo(id);
      setState(prev => ({
        ...prev,
        todos: prev.todos.map(t => t.id === id ? { ...t, isCompleted: false } : t),
        error: null,
      }));
    } catch (error) {
      setState(prev => ({ ...prev, error: error as ApiError }));
      throw error;
    }
  }, []);

  const deleteTodo = useCallback(async (id: number) => {
    try {
      await todoService.deleteTodo(id);
      setState(prev => ({
        ...prev,
        todos: prev.todos.filter(t => t.id !== id),
        error: null,
      }));
    } catch (error) {
      setState(prev => ({ ...prev, error: error as ApiError }));
      throw error;
    }
  }, []);

  const clearError = useCallback(() => {
    setState(prev => ({ ...prev, error: null }));
  }, []);

  useEffect(() => {
    fetchTodos();
  }, [fetchTodos]);

  return {
    todos: state.todos,
    loading: state.loading,
    error: state.error,
    params,
    setParams,
    fetchTodos,
    createTodo,
    updateTodo,
    completeTodo,
    reopenTodo,
    deleteTodo,
    clearError,
  };
};
