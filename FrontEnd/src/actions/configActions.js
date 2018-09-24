import * as types from '../constants/actionTypes'


const toggleSortable = isOpen => (
    {
        type: types.TOGGLE_SORTABLE_LIST,
        isOpen
    }
);

const onSortEnd = tableCols => (
    {
        type: types.ON_SORT_COLUMN_END,
        tableCols
    }
);

const changeOpenedTasks = openedArray => (
    {
        type: types.CHANGE_OPENED_TASKS,
        openedArray
    }
);

const changeSortMatches = (name, value) => (
    {
        type: types.CHANGE_SORT_MATCHES,
        name,
        value
    }
);

const changeLocale = locale => (
    {
        type: types.CHANGE_LOCALE,
        locale
    }
);

export {
    toggleSortable,
    onSortEnd,
    changeOpenedTasks,
    changeSortMatches,
    changeLocale
}