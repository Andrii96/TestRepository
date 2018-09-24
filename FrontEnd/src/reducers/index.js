import { combineReducers } from 'redux'
import currentUser from './currentUserReducer'
import filesList from './filesReducer'
import filters from './filtersReducer'
import groupedTasksList from './groupedTasksReducer'
import config from './configReducer'
import assigneeListsByTaskId from './assigneeListsByTaskReducer'
import surveysByTaskId from './surveysByTaskIdReducer'

import { reducer as toastr } from 'react-redux-toastr'

const rootReducer = combineReducers({
    toastr,
    currentUser,
    filesList,
    filters,
    groupedTasksList,
    config,
    assigneeListsByTaskId,
    surveysByTaskId
});

export default rootReducer