// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom';

// Mock react-i18next
jest.mock('react-i18next', () => ({
  useTranslation: () => {
    return {
      t: (key: string) => {
        const translations: Record<string, string> = {
          'app.title': 'My Todos',
          'app.add_todo': '+ Add Todo',
          'app.cancel': 'Cancel',
          'app.loading': 'Loading todos...',
          'app.no_todos': 'No todos found',
          'app.search_placeholder': 'Search todos...',
          'app.filter_all': 'All',
          'app.filter_active': 'Active',
          'app.filter_completed': 'Completed',
          'app.sort_by': 'Sort by:',
          'app.sort_title': 'Title',
          'app.sort_due_date': 'Due Date',
          'app.sort_newest': 'Newest',
          'todo.delete': 'Delete',
          'todo.deleting': 'Deleting...',
          'form.title_label': 'Title *',
          'form.description_label': 'Description',
          'form.due_date_label': 'Due Date',
          'form.create_button': 'Create Todo',
          'form.cancel_button': 'Cancel',
        };
        return translations[key] || key;
      },
      i18n: {
        changeLanguage: () => new Promise(() => {}),
      },
    };
  },
  initReactI18next: {
    type: '3rdParty',
    init: () => {},
  }
}));
