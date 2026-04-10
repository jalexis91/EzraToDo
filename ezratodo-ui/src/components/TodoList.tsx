import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useTodos } from '../hooks/useTodos';
import TodoItem from './TodoItem';
import TodoForm from './TodoForm';
import ErrorAlert from './common/ErrorAlert';

const TodoList: React.FC = () => {
  const { t } = useTranslation();
  const { 
    todos, 
    loading, 
    error, 
    params, 
    setParams, 
    createTodo, 
    completeTodo, 
    reopenTodo, 
    deleteTodo,
    clearError
  } = useTodos();

  const [showForm, setShowForm] = useState(false);
  const [searchInput, setSearchInput] = useState('');

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      setParams(prev => ({ ...prev, searchTerm: searchInput || undefined }));
    }, 500);
    return () => clearTimeout(timer);
  }, [searchInput, setParams]);

  const handleCreateTodo = async (title: string, description?: string, dueDate?: string) => {
    try {
      await createTodo({ title, description, dueDate });
      setShowForm(false);
    } catch {
      // Error handled by hook
    }
  };

  const handleToggleComplete = async (id: number, isCompleted: boolean) => {
    try {
      if (isCompleted) {
        await reopenTodo(id);
      } else {
        await completeTodo(id);
      }
    } catch {
      // Error handled by hook
    }
  };

  const handleFilterChange = (isCompleted?: boolean) => {
    setParams(prev => ({ ...prev, isCompleted }));
  };

  const handleSortChange = (sortBy: string) => {
    setParams(prev => {
      const isSameSort = prev.sortBy === sortBy;
      const nextOrder = isSameSort && prev.sortOrder === 'asc' ? 'desc' : 'asc';
      return { ...prev, sortBy, sortOrder: nextOrder };
    });
  };

  return (
    <main style={{ maxWidth: '42rem', margin: '0 auto', padding: '1rem' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ fontSize: '1.875rem', fontWeight: 700, margin: 0 }}>{t('app.title')}</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          aria-expanded={showForm}
          style={{
            padding: '0.5rem 1rem',
            backgroundColor: '#3b82f6',
            color: 'white',
            border: 'none',
            borderRadius: '0.375rem',
            cursor: 'pointer'
          }}
        >
          {showForm ? t('app.cancel') : t('app.add_todo')}
        </button>
      </header>

      {error && (
        <ErrorAlert
          title={error.title === 'Network error' ? t('app.error_network') : (error.title || t('app.error_title'))}
          errors={error.errors}
          onDismiss={clearError}
        />
      )}

      {showForm && (
        <section aria-label="Add new todo">
          <TodoForm 
            onSubmit={handleCreateTodo}
            onCancel={() => setShowForm(false)}
          />
        </section>
      )}

      <div style={{ marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
        <input
          type="text"
          placeholder={t('app.search_placeholder')}
          aria-label={t('app.search_placeholder')}
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          style={{
            padding: '0.5rem 0.75rem',
            border: '1px solid #d1d5db',
            borderRadius: '0.375rem',
            width: '100%',
            boxSizing: 'border-box'
          }}
        />

        <nav aria-label="Filters and sorting" style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '0.5rem' }}>
          <div role="group" aria-label="Filter by status" style={{ display: 'flex', gap: '0.25rem' }}>
            <button
              onClick={() => handleFilterChange(undefined)}
              aria-pressed={params.isCompleted === undefined}
              style={{
                padding: '0.25rem 0.75rem',
                backgroundColor: params.isCompleted === undefined ? '#3b82f6' : '#e5e7eb',
                color: params.isCompleted === undefined ? 'white' : '#374151',
                border: 'none',
                borderRadius: '0.375rem',
                cursor: 'pointer',
                fontSize: '0.875rem'
              }}
            >
              {t('app.filter_all')}
            </button>
            <button
              onClick={() => handleFilterChange(false)}
              aria-pressed={params.isCompleted === false}
              style={{
                padding: '0.25rem 0.75rem',
                backgroundColor: params.isCompleted === false ? '#3b82f6' : '#e5e7eb',
                color: params.isCompleted === false ? 'white' : '#374151',
                border: 'none',
                borderRadius: '0.375rem',
                cursor: 'pointer',
                fontSize: '0.875rem'
              }}
            >
              {t('app.filter_active')}
            </button>
            <button
              onClick={() => handleFilterChange(true)}
              aria-pressed={params.isCompleted === true}
              style={{
                padding: '0.25rem 0.75rem',
                backgroundColor: params.isCompleted === true ? '#3b82f6' : '#e5e7eb',
                color: params.isCompleted === true ? 'white' : '#374151',
                border: 'none',
                borderRadius: '0.375rem',
                cursor: 'pointer',
                fontSize: '0.875rem'
              }}
            >
              {t('app.filter_completed')}
            </button>
          </div>

          <div role="group" aria-label="Sort options" style={{ display: 'flex', gap: '0.25rem' }}>
            <span id="sort-label" style={{ fontSize: '0.875rem', color: '#6b7280', alignSelf: 'center' }}>{t('app.sort_by')}</span>
            <button
              onClick={() => handleSortChange('title')}
              aria-labelledby="sort-label"
              style={{
                fontSize: '0.875rem',
                background: 'none',
                border: 'none',
                color: params.sortBy === 'title' ? '#3b82f6' : '#374151',
                cursor: 'pointer',
                fontWeight: params.sortBy === 'title' ? 600 : 400
              }}
            >
              {t('app.sort_title')} {params.sortBy === 'title' && (params.sortOrder === 'asc' ? '↑' : '↓')}
            </button>
            <button
              onClick={() => handleSortChange('duedate')}
              aria-labelledby="sort-label"
              style={{
                fontSize: '0.875rem',
                background: 'none',
                border: 'none',
                color: params.sortBy === 'duedate' ? '#3b82f6' : '#374151',
                cursor: 'pointer',
                fontWeight: params.sortBy === 'duedate' ? 600 : 400
              }}
            >
              {t('app.sort_due_date')} {params.sortBy === 'duedate' && (params.sortOrder === 'asc' ? '↑' : '↓')}
            </button>
            <button
              onClick={() => handleSortChange('createdat')}
              aria-labelledby="sort-label"
              style={{
                fontSize: '0.875rem',
                background: 'none',
                border: 'none',
                color: (!params.sortBy || params.sortBy === 'createdat') ? '#3b82f6' : '#374151',
                cursor: 'pointer',
                fontWeight: (!params.sortBy || params.sortBy === 'createdat') ? 600 : 400
              }}
            >
              {t('app.sort_newest')} {(!params.sortBy || params.sortBy === 'createdat') && (params.sortOrder === 'asc' ? '↑' : '↓')}
            </button>
          </div>
        </nav>
      </div>

      <section aria-live="polite">
        {loading && todos.length === 0 ? (
          <p style={{ textAlign: 'center', color: '#6b7280', padding: '2rem' }}>{t('app.loading')}</p>
        ) : todos.length === 0 ? (
          <div style={{ textAlign: 'center', color: '#6b7280', padding: '3rem', border: '2px dashed #e5e7eb', borderRadius: '0.5rem' }}>
            <p style={{ fontSize: '1.125rem', margin: '0 0 0.5rem 0' }}>{t('app.no_todos')}</p>
            <p style={{ fontSize: '0.875rem', margin: 0 }}>{t('app.no_todos_detail')}</p>
          </div>
        ) : (
          <ul style={{ listStyleType: 'none', padding: 0, margin: 0 }}>
            {todos.map(todo => (
              <TodoItem
                key={todo.id}
                todo={todo}
                onToggleComplete={handleToggleComplete}
                onDelete={deleteTodo}
              />
            ))}
          </ul>
        )}
      </section>
    </main>
  );
};

export default TodoList;
