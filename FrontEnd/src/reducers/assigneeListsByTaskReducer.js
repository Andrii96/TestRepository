import initialState from './initialState'
import { GET_ASSIGNEE_LIST_BY_TASK_SUCCESS } from '../constants/actionTypes'

export default function(state = initialState.assigneeListsByTaskId, action) {
    switch (action.type) {
        case GET_ASSIGNEE_LIST_BY_TASK_SUCCESS:
            return {
                ...state,
                [action.taskId]: [...action.assigneeList]
            };
        default:
            return state
    }
}
