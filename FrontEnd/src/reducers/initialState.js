export default {
    currentUser: {},
    filesList: [],
    groupedTasksList: [],
    filters: {
        loadedTasks: false,
        searchField: '',
        late: false,
        unassigned: true,
        validatorsList: [],
        responsibilitiesList: [],
        groupsList: [],
        progressesList: []
    },
    assigneeListsByTaskId: {},
    surveysByTaskId: {},
    config: {
        locale: 'en_US',
        isOpenSortable: false,
        tableCols: [
            {id: 1, name: 'gridProjectCode', fields: ['Project', 'ProjectCode'], visible: true},
            {id: 2, name: 'gridProjectDeadline', fields: ['DeadLine'], visible: true, isDate: true},
            {id: 3, name: 'gridRequestDate', fields: ['RequestDate'], visible: true, isDate: true},
            {id: 4, name: 'gridSourceLanguage', fields: ['SourceLanguage'], visible: true},
            {id: 5, name: 'gridTargetLanguage', fields: ['TargetLanguage'], visible: true},
            {id: 6, name: 'gridAssignee', fields: ['Assignee', 'Name'], visible: true, emailLink: 'Email'},
            {id: 7, name: 'gridStatus', fields: ['Status'], visible: true},
            {id: 8, name: 'gridResponsible', fields: ['Responsible', 'Name'], visible: true, emailLink: 'Email'},
            {id: 9, name: 'gridProjectName', fields: ['Project', 'Name'], visible: true},
            {id: 10, name: 'gridTaskCode', fields: ['TaskCode'], visible: true}
        ],
        openedArray: [],
        matches: [
            {value: '<=', name: '<=', selected: true},
            {value: '<', name: '<', selected: false},
            {value: '>', name: '>', selected: false},
            {value: '=>', name: '=>', selected: false}
        ],
        percentage: [
            {value: '50', name: '0', selected: false},
            {value: '60', name: '0.6', selected: false},
            {value: '70', name: '0.7', selected: false},
            {value: '80', name: '0.8', selected: false},
            {value: '90', name: '0.9', selected: false},
            {value: '100', name: '1', selected: true}
        ]
    }
}