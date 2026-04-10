# Frontend Architecture & Component Design

This skill file covers React/Vue best practices, component patterns, state management, and API communication for the EzraToDo frontend.

> **See Also**: [`take-home-test-objectives.md`](./take-home-test-objectives.md) for evaluation criteria on frontend component design, frontend-backend communication, and testing strategy. This guide provides the specific patterns and examples to meet those criteria.

## Architecture Philosophy

- **Component-Based**: Break UI into reusable, focused components
- **Separation of Concerns**: Keep components, services, and utilities separate
- **Single Responsibility**: Each component has one clear purpose
- **Testability**: Design components to be easily testable in isolation
- **Performance**: Minimize re-renders, use proper memoization
- **Accessibility**: Build with semantic HTML and ARIA labels

## Project Structure

```
frontend/
├── src/
│   ├── components/
│   │   ├── TodoList.tsx
│   │   ├── TodoItem.tsx
│   │   ├── TodoForm.tsx
│   │   └── common/
│   │       ├── Button.tsx
│   │       └── ErrorAlert.tsx
│   ├── hooks/
│   │   ├── useTodos.ts
│   │   └── useAsync.ts
│   ├── services/
│   │   └── todoService.ts
│   ├── types/
│   │   └── todo.ts
│   ├── App.tsx
│   ├── App.css
│   └── index.tsx
├── public/
│   └── index.html
├── package.json
└── .env.example
```

## API Service Layer (React/TypeScript Example)

The API service encapsulates all HTTP communication, making it easy to mock and test.

### Types Definition

```typescript
// types/todo.ts
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
```

### API Service

```typescript
// services/todoService.ts
import axios, { AxiosError } from 'axios';
import { Todo, CreateTodoRequest, UpdateTodoRequest, ApiError } from '../types/todo';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const todoService = {
  async getTodos(): Promise<Todo[]> {
    try {
      const response = await apiClient.get<Todo[]>('/todos');
      return response.data;
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

  async deleteTodo(id: number): Promise<void> {
    try {
      await apiClient.delete(`/todos/${id}`);
    } catch (error) {
      throw this.handleError(error);
    }
  },

  private handleError(error: unknown): ApiError {
    if (axios.isAxiosError(error)) {
      const data = error.response?.data as ApiError;
      return {
        type: data?.type || 'unknown-error',
        title: data?.title || 'An error occurred',
        status: error.response?.status || 500,
        errors: data?.errors,
      };
    }
    return {
      type: 'network-error',
      title: 'Network error',
      status: 0,
    };
  },
};
```

## State Management with Hooks (React)

Use custom hooks to manage todo list state and API calls.

### useTodos Hook

```typescript
// hooks/useTodos.ts
import { useState, useEffect, useCallback } from 'react';
import { todoService, Todo, CreateTodoRequest, UpdateTodoRequest, ApiError } from '../services/todoService';

interface UseTodosState {
  todos: Todo[];
  loading: boolean;
  error: ApiError | null;
}

export const useTodos = () => {
  const [state, setState] = useState<UseTodosState>({
    todos: [],
    loading: false,
    error: null,
  });

  // Fetch all todos
  const fetchTodos = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    try {
      const todos = await todoService.getTodos();
      setState(prev => ({ ...prev, todos, loading: false }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        error: error as ApiError,
        loading: false,
      }));
    }
  }, []);

  // Create a new todo
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

  // Update a todo
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

  // Delete a todo
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

  // Load todos on mount
  useEffect(() => {
    fetchTodos();
  }, [fetchTodos]);

  return {
    todos: state.todos,
    loading: state.loading,
    error: state.error,
    fetchTodos,
    createTodo,
    updateTodo,
    deleteTodo,
  };
};
```

## Component Patterns

### Main List Component

```typescript
// components/TodoList.tsx
import React, { useState } from 'react';
import { useTodos } from '../hooks/useTodos';
import TodoItem from './TodoItem';
import TodoForm from './TodoForm';
import ErrorAlert from './common/ErrorAlert';

type FilterType = 'all' | 'active' | 'completed';

const TodoList: React.FC = () => {
  const { todos, loading, error, createTodo, updateTodo, deleteTodo } = useTodos();
  const [filter, setFilter] = useState<FilterType>('all');
  const [showForm, setShowForm] = useState(false);

  // Filter todos based on completion status
  const filteredTodos = todos.filter(t => {
    if (filter === 'active') return !t.isCompleted;
    if (filter === 'completed') return t.isCompleted;
    return true;
  });

  // Handle form submission
  const handleCreateTodo = async (title: string, description?: string) => {
    try {
      await createTodo({ title, description });
      setShowForm(false);
    } catch {
      // Error is already in state
    }
  };

  // Handle mark complete
  const handleToggleComplete = async (id: number, isCompleted: boolean) => {
    try {
      await updateTodo(id, { isCompleted: !isCompleted });
    } catch {
      // Error is already in state
    }
  };

  if (loading) {
    return <div className="p-4 text-center">Loading todos...</div>;
  }

  return (
    <div className="max-w-2xl mx-auto p-4">
      <h1 className="text-3xl font-bold mb-4">My Todos</h1>

      {/* Error Alert */}
      {error && (
        <ErrorAlert
          title={error.title}
          errors={error.errors}
          onDismiss={() => {}}
        />
      )}

      {/* Create Todo Form */}
      {showForm ? (
        <TodoForm 
          onSubmit={handleCreateTodo}
          onCancel={() => setShowForm(false)}
        />
      ) : (
        <button
          onClick={() => setShowForm(true)}
          className="mb-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          + Add Todo
        </button>
      )}

      {/* Filter Buttons */}
      <div className="flex gap-2 my-4">
        {(['all', 'active', 'completed'] as const).map(f => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className={`px-3 py-1 rounded ${
              filter === f
                ? 'bg-blue-500 text-white'
                : 'bg-gray-200 hover:bg-gray-300'
            }`}
          >
            {f.charAt(0).toUpperCase() + f.slice(1)}
          </button>
        ))}
      </div>

      {/* Todo List */}
      {filteredTodos.length === 0 ? (
        <p className="text-center text-gray-500 py-8">
          No todos found. {filter !== 'all' && 'Try a different filter.'}
        </p>
      ) : (
        <ul className="space-y-2">
          {filteredTodos.map(todo => (
            <TodoItem
              key={todo.id}
              todo={todo}
              onToggleComplete={handleToggleComplete}
              onDelete={deleteTodo}
            />
          ))}
        </ul>
      )}
    </div>
  );
};

export default TodoList;
```

### Individual Todo Item Component

```typescript
// components/TodoItem.tsx
import React, { useState } from 'react';
import { Todo } from '../types/todo';

interface TodoItemProps {
  todo: Todo;
  onToggleComplete: (id: number, isCompleted: boolean) => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}

const TodoItem: React.FC<TodoItemProps> = ({ todo, onToggleComplete, onDelete }) => {
  const [isDeleting, setIsDeleting] = useState(false);
  const [isToggling, setIsToggling] = useState(false);

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this todo?')) {
      setIsDeleting(true);
      try {
        await onDelete(todo.id);
      } finally {
        setIsDeleting(false);
      }
    }
  };

  const handleToggle = async () => {
    setIsToggling(true);
    try {
      await onToggleComplete(todo.id, todo.isCompleted);
    } finally {
      setIsToggling(false);
    }
  };

  return (
    <li className="flex items-center gap-3 p-3 bg-white rounded border hover:shadow-md transition-shadow">
      <input
        type="checkbox"
        checked={todo.isCompleted}
        onChange={handleToggle}
        disabled={isToggling}
        className="w-5 h-5 rounded"
        aria-label={`Mark "${todo.title}" as ${todo.isCompleted ? 'incomplete' : 'complete'}`}
      />
      <div className="flex-1">
        <h3 className={`font-semibold ${todo.isCompleted ? 'line-through text-gray-400' : ''}`}>
          {todo.title}
        </h3>
        {todo.description && (
          <p className="text-sm text-gray-600">{todo.description}</p>
        )}
        {todo.dueDate && (
          <p className="text-xs text-gray-500">
            Due: {new Date(todo.dueDate).toLocaleDateString()}
          </p>
        )}
      </div>
      <button
        onClick={handleDelete}
        disabled={isDeleting}
        className="px-3 py-1 text-red-600 hover:bg-red-50 rounded disabled:opacity-50"
        aria-label={`Delete todo "${todo.title}"`}
      >
        {isDeleting ? 'Deleting...' : 'Delete'}
      </button>
    </li>
  );
};

export default TodoItem;
```

### Todo Form Component

```typescript
// components/TodoForm.tsx
import React, { useState } from 'react';

interface TodoFormProps {
  onSubmit: (title: string, description?: string) => Promise<void>;
  onCancel: () => void;
}

const TodoForm: React.FC<TodoFormProps> = ({ onSubmit, onCancel }) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!title.trim()) {
      newErrors.title = 'Title is required';
    } else if (title.length > 200) {
      newErrors.title = 'Title must be 200 characters or less';
    }

    if (description && description.length > 2000) {
      newErrors.description = 'Description must be 2000 characters or less';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit(title, description || undefined);
      setTitle('');
      setDescription('');
    } catch (error) {
      // Error handling is done in parent component
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="mb-4 p-4 bg-gray-50 rounded border">
      <div className="mb-3">
        <label htmlFor="title" className="block text-sm font-medium mb-1">
          Title *
        </label>
        <input
          id="title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Enter todo title"
          maxLength={200}
          className={`w-full px-3 py-2 border rounded ${
            errors.title ? 'border-red-500' : 'border-gray-300'
          }`}
          disabled={isSubmitting}
        />
        {errors.title && (
          <p className="text-sm text-red-600 mt-1">{errors.title}</p>
        )}
        <p className="text-xs text-gray-500 mt-1">{title.length}/200</p>
      </div>

      <div className="mb-3">
        <label htmlFor="description" className="block text-sm font-medium mb-1">
          Description
        </label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Enter optional description"
          maxLength={2000}
          rows={3}
          className={`w-full px-3 py-2 border rounded ${
            errors.description ? 'border-red-500' : 'border-gray-300'
          }`}
          disabled={isSubmitting}
        />
        {errors.description && (
          <p className="text-sm text-red-600 mt-1">{errors.description}</p>
        )}
        <p className="text-xs text-gray-500 mt-1">{description.length}/2000</p>
      </div>

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50"
        >
          {isSubmitting ? 'Creating...' : 'Create Todo'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          className="px-4 py-2 bg-gray-300 text-gray-800 rounded hover:bg-gray-400 disabled:opacity-50"
        >
          Cancel
        </button>
      </div>
    </form>
  );
};

export default TodoForm;
```

### Error Alert Component

```typescript
// components/common/ErrorAlert.tsx
import React from 'react';
import { ApiError } from '../../types/todo';

interface ErrorAlertProps {
  title: string;
  errors?: Record<string, string[]>;
  onDismiss: () => void;
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({ title, errors, onDismiss }) => {
  return (
    <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded">
      <div className="flex justify-between items-start">
        <div>
          <h3 className="font-semibold text-red-800">{title}</h3>
          {errors && Object.keys(errors).length > 0 && (
            <ul className="mt-2 list-disc list-inside">
              {Object.entries(errors).map(([field, messages]) => (
                <li key={field} className="text-sm text-red-700">
                  <strong>{field}:</strong> {messages.join(', ')}
                </li>
              ))}
            </ul>
          )}
        </div>
        <button
          onClick={onDismiss}
          className="text-red-600 hover:text-red-800 font-semibold"
          aria-label="Dismiss error"
        >
          ×
        </button>
      </div>
    </div>
  );
};

export default ErrorAlert;
```

## Environment Configuration

### .env.example

```
REACT_APP_API_URL=http://localhost:5000/api
```

### Development vs. Production

**package.json scripts**:
```json
{
  "scripts": {
    "start": "react-scripts start",
    "build": "react-scripts build",
    "test": "react-scripts test",
    "eject": "react-scripts eject"
  }
}
```

For different environments:
- **Development**: `REACT_APP_API_URL=http://localhost:5000/api`
- **Staging**: `REACT_APP_API_URL=https://staging-api.example.com/api`
- **Production**: `REACT_APP_API_URL=https://api.example.com/api`

## Error Handling Patterns

### Error Handling in Hooks

```typescript
// Errors are handled in useTodos and stored in state
const { todos, loading, error } = useTodos();

// Display error
if (error) {
  return <ErrorAlert title={error.title} errors={error.errors} />;
}
```

### Network Error Handling

```typescript
// API service catches and formats errors
private handleError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    return {
      type: error.response?.data?.type || 'network-error',
      title: error.response?.data?.title || 'An error occurred',
      status: error.response?.status || 500,
      errors: error.response?.data?.errors,
    };
  }
  return {
    type: 'unknown-error',
    title: 'An unexpected error occurred',
    status: 500,
  };
}
```

## Styling Approach

**Recommended for MVP**: Tailwind CSS (utility-first, minimal setup)

```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

**Alternative**: CSS Modules or styled-components

## Testing Components

### Component Test Example

```typescript
// components/__tests__/TodoItem.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import TodoItem from '../TodoItem';
import { Todo } from '../../types/todo';

describe('TodoItem', () => {
  const mockTodo: Todo = {
    id: 1,
    title: 'Test todo',
    isCompleted: false,
    createdAt: '2026-03-25T00:00:00Z',
    updatedAt: '2026-03-25T00:00:00Z',
  };

  it('renders todo title', () => {
    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={jest.fn()}
        onDelete={jest.fn()}
      />
    );

    expect(screen.getByText('Test todo')).toBeInTheDocument();
  });

  it('calls onToggleComplete when checkbox is clicked', async () => {
    const onToggleComplete = jest.fn();

    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={onToggleComplete}
        onDelete={jest.fn()}
      />
    );

    const checkbox = screen.getByRole('checkbox');
    fireEvent.click(checkbox);

    expect(onToggleComplete).toHaveBeenCalledWith(1, false);
  });
});
```

## Performance Best Practices

- **Memoization**: Use `React.memo()` for TodoItem to prevent unnecessary re-renders
- **useCallback**: Memoize callbacks in useTodos to prevent circular updates
- **Key Props**: Always use stable IDs in lists (not array index)
- **Code Splitting**: Lazy load components if app grows

```typescript
const TodoList = lazy(() => import('./TodoList'));
```

## Accessibility Standards

- **Semantic HTML**: Use `<button>`, `<form>`, `<label>` tags properly
- **ARIA Labels**: Add aria-label for icon-only buttons
- **Keyboard Navigation**: Ensure all interactive elements are keyboard-accessible
- **Focus Management**: Manage focus when modals/forms open
- **Color Contrast**: Ensure text meets WCAG AA standards

## Vue.js Alternative

For Vue developers, the patterns are similar:

```vue
<template>
  <div class="max-w-2xl mx-auto p-4">
    <h1 class="text-3xl font-bold mb-4">My Todos</h1>
    <ErrorAlert v-if="error" :error="error" />
    <TodoForm @submit="createTodo" />
    <TodoList :todos="todos" @delete="deleteTodo" @toggle="updateTodo" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useTodos } from './composables/useTodos';

const { todos, error, createTodo, deleteTodo, updateTodo } = useTodos();
</script>
```

