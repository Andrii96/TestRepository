import initialState from './initialState'
import {
    GET_ALL_FILTER_USERS_SUCCESS,
    GET_ALL_FILTER_GROUPS_SUCCESS,
    GET_ALL_FILTER_STATUSES_SUCCESS,
    CHANGE_FILTER,
    CHANGE_ALL_FILTER_LIST,
    CHANGE_LOADED_TASK_STATUS
} from '../constants/actionTypes'

export default function(state = initialState.filters, action) {
    switch (action.type) {
        case GET_ALL_FILTER_USERS_SUCCESS:
            const checkedList = action.usersList.map(i => ({...i, checked: true}));
            return {
                ...state,
                responsibilitiesList: checkedList,
                validatorsList: checkedList
            };
        case GET_ALL_FILTER_GROUPS_SUCCESS:
            const checkedGroupsList = action.groupsList.map(i => (i.Id === 0) ? {...i, checked: true} : i );
            return {
                ...state,
                groupsList: checkedGroupsList
            };
        case GET_ALL_FILTER_STATUSES_SUCCESS:
            const checkedProgressesList = action.progressesList.map(i => ({...i, checked: true}));
            return {
                ...state,
                progressesList: checkedProgressesList
            };
        case CHANGE_FILTER:
            return {
                ...state,
                [action.filterType]: action.value
            };
        case CHANGE_ALL_FILTER_LIST:
            const oldList = state[action.filterType];
            const newList = oldList.map(i => ({...i, checked: action.isChecked}));
            return {
                ...state,
                [action.filterType]: newList
            };
        case CHANGE_LOADED_TASK_STATUS:
            return {
                ...state,
                loadedTasks: action.isLoaded
            };
        default:
            return state
    }
}