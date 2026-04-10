import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Todo } from '../types/todo';

interface TodoItemProps {
  todo: Todo;
  onToggleComplete: (id: number, isCompleted: boolean) => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}

const TodoItem: React.FC<TodoItemProps> = ({ todo, onToggleComplete, onDelete }) => {
  const { t } = useTranslation();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isToggling, setIsToggling] = useState(false);

  const handleDelete = async () => {
    if (window.confirm(t('todo.delete_confirm', { title: todo.title }))) {
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

  const formatDate = (dateString?: string) => {
    if (!dateString) return null;
    return new Date(dateString).toLocaleDateString();
  };

  return (
    <li style={{
      display: 'flex',
      alignItems: 'center',
      gap: '0.75rem',
      padding: '0.75rem',
      backgroundColor: 'white',
      borderRadius: '0.375rem',
      border: '1px solid #e5e7eb',
      marginBottom: '0.5rem',
      transition: 'box-shadow 0.2s',
      boxShadow: '0 1px 2px 0 rgba(0, 0, 0, 0.05)'
    }}>
      <input
        type="checkbox"
        checked={todo.isCompleted}
        onChange={handleToggle}
        disabled={isToggling}
        style={{
          width: '1.25rem',
          height: '1.25rem',
          cursor: 'pointer'
        }}
        aria-label={todo.isCompleted 
          ? t('todo.mark_incomplete', { title: todo.title }) 
          : t('todo.mark_complete', { title: todo.title })}
      />
      <div style={{ flex: 1 }}>
        <h3 style={{
          fontSize: '1rem',
          fontWeight: 600,
          margin: 0,
          textDecoration: todo.isCompleted ? 'line-through' : 'none',
          color: todo.isCompleted ? '#9ca3af' : '#111827'
        }}>
          {todo.title}
        </h3>
        {todo.description && (
          <p style={{
            fontSize: '0.875rem',
            color: '#4b5563',
            margin: '0.25rem 0 0 0'
          }}>
            {todo.description}
          </p>
        )}
        {todo.dueDate && (
          <p style={{
            fontSize: '0.75rem',
            color: '#6b7280',
            margin: '0.25rem 0 0 0'
          }}>
            {t('todo.due', { date: formatDate(todo.dueDate) })}
          </p>
        )}
      </div>
      <button
        onClick={handleDelete}
        disabled={isDeleting}
        style={{
          padding: '0.25rem 0.75rem',
          color: '#dc2626',
          backgroundColor: 'transparent',
          border: '1px solid transparent',
          borderRadius: '0.375rem',
          cursor: 'pointer',
          fontSize: '0.875rem',
          opacity: isDeleting ? 0.5 : 1
        }}
        onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#fef2f2'}
        onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
        aria-label={`${t('todo.delete')} "${todo.title}"`}
      >
        {isDeleting ? t('todo.deleting') : t('todo.delete')}
      </button>
    </li>
  );
};

export default TodoItem;
