import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

const resources = {
  en: {
    translation: {
      "app": {
        "title": "My Todos",
        "loading": "Loading todos...",
        "no_todos": "No todos found",
        "no_todos_detail": "Try adjusting your filters or search term.",
        "search_placeholder": "Search todos...",
        "add_todo": "+ Add Todo",
        "cancel": "Cancel",
        "sort_by": "Sort by:",
        "sort_title": "Title",
        "sort_due_date": "Due Date",
        "sort_newest": "Newest",
        "filter_all": "All",
        "filter_active": "Active",
        "filter_completed": "Completed",
        "error_title": "Error",
        "error_network": "Network error. Please check your connection.",
        "error_unexpected": "An unexpected error occurred. Please try again."
      },
      "todo": {
        "due": "Due: {{date}}",
        "mark_complete": "Mark \"{{title}}\" as complete",
        "mark_incomplete": "Mark \"{{title}}\" as incomplete",
        "delete": "Delete",
        "deleting": "Deleting...",
        "delete_confirm": "Are you sure you want to delete \"{{title}}\"?"
      },
      "form": {
        "title_label": "Title *",
        "title_placeholder": "Enter todo title",
        "description_label": "Description",
        "description_placeholder": "Enter optional description",
        "due_date_label": "Due Date",
        "create_button": "Create Todo",
        "creating_button": "Creating...",
        "cancel_button": "Cancel",
        "validation": {
          "title_required": "Title is required",
          "title_too_long": "Title must be 200 characters or less",
          "description_too_long": "Description must be 2000 characters or less"
        }
      }
    }
  }
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false
    }
  });

export default i18n;
