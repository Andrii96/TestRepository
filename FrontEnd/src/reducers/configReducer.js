import initialState from './initialState'
import {
    TOGGLE_SORTABLE_LIST,
    ON_SORT_COLUMN_END,
    CHANGE_OPENED_TASKS,
    CHANGE_SORT_MATCHES,
    CHANGE_LOCALE
} from '../constants/actionTypes'

export default function(state = initialState.config, action) {
    switch (action.type) {
        case TOGGLE_SORTABLE_LIST:
            return {
                ...state,
                isOpenSortable: action.isOpen
            };
        case ON_SORT_COLUMN_END:
            localStorage.setItem('tableCols', JSON.stringify(action.tableCols));
            return  {
                ...state,
                tableCols: action.tableCols
            };
        case CHANGE_OPENED_TASKS:
            localStorage.setItem('openedArray', JSON.stringify(action.openedArray));
            return  {
                ...state,
                openedArray: action.openedArray
            };
        case CHANGE_SORT_MATCHES:
            const newValue = state[action.name].map(i => ({...i, selected: i.value === action.value}));
            localStorage.setItem(`${action.name}`, JSON.stringify(newValue));
            return {
                ...state,
                [action.name]: newValue
            };
        case CHANGE_LOCALE:
            return {
                ...state,
                locale: action.locale
            };
        default:
            return state
    }
}
