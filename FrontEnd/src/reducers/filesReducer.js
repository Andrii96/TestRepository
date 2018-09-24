import initialState from './initialState'
import { IMPORT_FILE_SUCCESS, GET_ALL_TASKS_SUCCESS } from '../constants/actionTypes'

export default function(state = initialState.filesList, action) {
    switch (action.type) {
        case GET_ALL_TASKS_SUCCESS:
            return action.files;
        case IMPORT_FILE_SUCCESS:
            return [...state, action.file];
        default:
            return state
    }
}
