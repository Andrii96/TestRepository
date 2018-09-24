import initialState from './initialState'
import {
    GET_ALL_FILTERED_TASKS_SUCCESS,
    LOAD_FILES_FOR_TASK_SUCCESS,
    REASSIGN_TASK_SUCCESS,
    FINALIZE_TASK_SUCCESS
} from '../constants/actionTypes'

export default function(state = initialState.groupedTasksList, action) {
    switch (action.type) {
        case GET_ALL_FILTERED_TASKS_SUCCESS:
            return action.groupedTasksList;
        case LOAD_FILES_FOR_TASK_SUCCESS:
            const {ValidationFiles, ReferenceFiles} = action.data;
            return state.map(group => (
                {
                    ...group,
                    Tasks: group.Tasks.map(task => (
                        task.TaskId === action.taskId
                            ? {
                                ...task,
                                filesList: ValidationFiles,
                                referencesList: ReferenceFiles
                            }
                            : task
                    ))
                }
            ));
        case REASSIGN_TASK_SUCCESS:
            return state.map(group => (
                {
                    ...group,
                    Tasks: group.Tasks.map(task => (
                        task.TaskId === action.task.TaskId
                            ? {...action.task, fileList: task.files}
                            : task
                    ))
                }
            ));
        case FINALIZE_TASK_SUCCESS:
            return state.map(group => (
                {
                    ...group,
                    Tasks: group.Tasks.map(task => (
                        task.TaskId === action.task.TaskId
                            ? {...action.task, fileList: task.files}
                            : task
                    ))
                }
            ));
        default:
            return state
    }
}
