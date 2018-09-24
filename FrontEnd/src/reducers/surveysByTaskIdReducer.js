import initialState from './initialState'
import {
    GET_SURVEY_BY_TASK_SUCCESS,
    SEND_SURVEY_BY_TASK_SUCCESS
} from '../constants/actionTypes'

export default function(state = initialState.surveysByTaskId, action) {
    switch (action.type) {
        case GET_SURVEY_BY_TASK_SUCCESS:
        case SEND_SURVEY_BY_TASK_SUCCESS:
            return {
                ...state,
                [action.taskId]: action.survey
            };
        default:
            return state
    }
}
