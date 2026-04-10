import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface TodoFormProps {
  onSubmit: (title: string, description?: string, dueDate?: string) => Promise<void>;
  onCancel: () => void;
}

const TodoForm: React.FC<TodoFormProps> = ({ onSubmit, onCancel }) => {
  const { t } = useTranslation();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!title.trim()) {
      newErrors.title = t('form.validation.title_required');
    } else if (title.length > 200) {
      newErrors.title = t('form.validation.title_too_long');
    }

    if (description && description.length > 2000) {
      newErrors.description = t('form.validation.description_too_long');
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
      await onSubmit(title, description || undefined, dueDate || undefined);
      setTitle('');
      setDescription('');
      setDueDate('');
    } catch (error) {
      // Error handling is done in parent component
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{
      marginBottom: '1rem',
      padding: '1rem',
      backgroundColor: '#f9fafb',
      border: '1px solid #e5e7eb',
      borderRadius: '0.375rem'
    }}>
      <div style={{ marginBottom: '0.75rem' }}>
        <label htmlFor="title" style={{ display: 'block', fontSize: '0.875rem', fontWeight: 500, marginBottom: '0.25rem' }}>
          {t('form.title_label')}
        </label>
        <input
          id="title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder={t('form.title_placeholder')}
          maxLength={200}
          aria-invalid={!!errors.title}
          aria-describedby={errors.title ? "title-error" : undefined}
          required
          style={{
            width: '100%',
            padding: '0.5rem 0.75rem',
            border: `1px solid ${errors.title ? '#ef4444' : '#d1d5db'}`,
            borderRadius: '0.375rem',
            boxSizing: 'border-box'
          }}
          disabled={isSubmitting}
        />
        {errors.title && (
          <p id="title-error" style={{ fontSize: '0.875rem', color: '#dc2626', marginTop: '0.25rem' }}>{errors.title}</p>
        )}
      </div>

      <div style={{ marginBottom: '0.75rem' }}>
        <label htmlFor="description" style={{ display: 'block', fontSize: '0.875rem', fontWeight: 500, marginBottom: '0.25rem' }}>
          {t('form.description_label')}
        </label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder={t('form.description_placeholder')}
          maxLength={2000}
          rows={3}
          aria-invalid={!!errors.description}
          aria-describedby={errors.description ? "description-error" : undefined}
          style={{
            width: '100%',
            padding: '0.5rem 0.75rem',
            border: `1px solid ${errors.description ? '#ef4444' : '#d1d5db'}`,
            borderRadius: '0.375rem',
            boxSizing: 'border-box'
          }}
          disabled={isSubmitting}
        />
        {errors.description && (
          <p id="description-error" style={{ fontSize: '0.875rem', color: '#dc2626', marginTop: '0.25rem' }}>{errors.description}</p>
        )}
      </div>

      <div style={{ marginBottom: '0.75rem' }}>
        <label htmlFor="dueDate" style={{ display: 'block', fontSize: '0.875rem', fontWeight: 500, marginBottom: '0.25rem' }}>
          {t('form.due_date_label')}
        </label>
        <input
          id="dueDate"
          type="date"
          value={dueDate}
          onChange={(e) => setDueDate(e.target.value)}
          style={{
            width: '100%',
            padding: '0.5rem 0.75rem',
            border: '1px solid #d1d5db',
            borderRadius: '0.375rem',
            boxSizing: 'border-box'
          }}
          disabled={isSubmitting}
        />
      </div>

      <div style={{ display: 'flex', gap: '0.5rem' }}>
        <button
          type="submit"
          disabled={isSubmitting}
          style={{
            padding: '0.5rem 1rem',
            backgroundColor: '#3b82f6',
            color: 'white',
            border: 'none',
            borderRadius: '0.375rem',
            cursor: 'pointer',
            opacity: isSubmitting ? 0.5 : 1
          }}
        >
          {isSubmitting ? t('form.creating_button') : t('form.create_button')}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          style={{
            padding: '0.5rem 1rem',
            backgroundColor: '#d1d5db',
            color: '#1f2937',
            border: 'none',
            borderRadius: '0.375rem',
            cursor: 'pointer',
            opacity: isSubmitting ? 0.5 : 1
          }}
        >
          {t('form.cancel_button')}
        </button>
      </div>
    </form>
  );
};

export default TodoForm;