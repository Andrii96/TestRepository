import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'

import TLSelectList from '../TLSelectList'
import * as filtersActions from '../../actions/filtersActions'
import * as groupedTasksActions from '../../actions/groupedTasksActions'

const messages = defineMessages({
    validatorsHint: {
        id: 'taskFilers.validatorsHint',
        defaultMessage: 'Validators'
    },
    responsibilitiesHint: {
        id: 'taskFilters.responsibilitiesHint',
        defaultMessage: 'Responsibilities'
    },
    progressHint: {
        id: 'taskFilters.progressHint',
        defaultMessage: 'Progress'
    },
    groupHint: {
        id: 'taskFilters.groupHint',
        defaultMessage: 'Group'
    },
    searchPlaceholder: {
        id: 'taskFilters.searchPlaceholder',
        defaultMessage: 'Proj. code, name or task code'
    }
});

class TaskFilters extends Component {
    constructor(props) {
        super(props);

        this.applyFilters = this.applyFilters.bind(this);
        this.changeLate = this.changeLate.bind(this);
        this.changeUnassigned = this.changeUnassigned.bind(this);
        this.onChangeSearch = this.onChangeSearch.bind(this);
    }

    componentDidMount() {
        const {currentUser, filtersActions, validatorsList, responsibilitiesList, progressesList, groupsList} = this.props;

        if (!validatorsList[0] && !responsibilitiesList[0]) filtersActions.getAllStatus();
        if (!groupsList[0]) filtersActions.getAllGroups();
        if (!progressesList[0]) filtersActions.getAllUsers({
            ContactId: currentUser.ContactId,
            CompanyForEntityId: currentUser.Company && currentUser.Company.CompanyForEntityId
        });
    }

    componentWillReceiveProps(props) {
        const {validatorsList, responsibilitiesList, progressesList, groupsList, groupedTasksList, loadedTasks} = props;

        if (!validatorsList[0] || !responsibilitiesList[0] || !progressesList[0] || !groupsList[0]) return;

        if (groupedTasksList[0] || loadedTasks) return;

        this.applyFilters({
            validatorsList,
            responsibilitiesList,
            progressesList,
            groupsList
        }).call(this);
    }

    onChangeFilters(type) {
        return items => {
            this.props.filtersActions.changeFilter(type, items);
        };
    }

    onChangeAllFilters(type, markAll) {
        return () => {
            this.props.filtersActions.changeAllFilterList(type, markAll);
        };
    }

    changeLate() {
        this.props.filtersActions.changeFilter('late', !this.props.late);
    }

    changeUnassigned(value) {
        this.props.filtersActions.changeFilter('unassigned', (value || !this.props.unassigned) );
    }

    onChangeSearch(e) {
        this.props.filtersActions.changeFilter('searchField', e.target.value);
    }

    applyFilters(value) {
        return () => {
            const {groupedTasksActions, currentUser, validatorsList, responsibilitiesList, groupsList, progressesList,
                late, searchField, unassigned} = this.props;

            const ContactIds = (value ? value.validatorsList : validatorsList).filter(user => user.checked).map(user => user.ContactId);
            const ProjectResponsibleIds = (value ? value.responsibilitiesList : responsibilitiesList).filter(user => user.checked).map(user => user.ContactId);
            const TaskExecutionStatusIds = (value ? value.progressesList : progressesList).filter(i => i.checked).map(i => i.TaskExecutionStatusId);
            const groupColumn = (value ? value.groupsList : groupsList).filter(i => i.checked).map(i => i.Id);

            const data = {
                PageNumber: 0,
                PageSize: 1000, // TODO: Rewrite this
                ContactIds,
                ProjectResponsibleIds,
                TaskExecutionStatusIds: late ? [] : TaskExecutionStatusIds,
                TextSearch: searchField,
                Unassigned: unassigned,
                Late: late,
                GroupColumnId: groupColumn[0],
                CompanyForEntityId: currentUser.Company && currentUser.Company.CompanyForEntityId
            };

            groupedTasksActions.getAllTasksByFilter(data);
        }
    }


    render() {
        const {validatorsList, responsibilitiesList, progressesList, groupsList, late, searchField, unassigned,
            loadedTasks, intl: {formatMessage}} = this.props;

        return (
            <div className="tl-validation-list-filters">
                <div className="tl-flex-row">
                    <div className="tl-flex-row tl-flex-col tl-search-headers">
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.validators"
                                    defaultMessage="Validators"
                                />
                                <span className="tl-primary-color tl-required"> *</span>
                            </span>
                            <TLSelectList
                                hint={formatMessage(messages.validatorsHint)}
                                items={validatorsList}
                                onSelect={this.onChangeFilters('validatorsList')}
                                markAll={this.onChangeAllFilters('validatorsList', true)}
                                clearAll={this.onChangeAllFilters('validatorsList', false)}
                                multiSelection
                                nameToDisplay="Name"
                                unassignedItem={true}
                                unassignedValue={unassigned}
                                unassignedFunc={this.changeUnassigned}
                            />
                        </div>
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.responsibilities"
                                    defaultMessage="Responsibilities"
                                />
                                <span className="tl-primary-color tl-required"> *</span>
                            </span>
                            <TLSelectList
                                hint={formatMessage(messages.responsibilitiesHint)}
                                items={responsibilitiesList}
                                onSelect={this.onChangeFilters('responsibilitiesList')}
                                markAll={this.onChangeAllFilters('responsibilitiesList', true)}
                                clearAll={this.onChangeAllFilters('responsibilitiesList', false)}
                                multiSelection
                                nameToDisplay="Name"
                            />
                        </div>
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.progress"
                                    defaultMessage="Progress"
                                />
                                <span className="tl-primary-color tl-required"> *</span>
                            </span>
                            <TLSelectList
                                hint={formatMessage(messages.progressHint)}
                                items={progressesList}
                                onSelect={this.onChangeFilters('progressesList')}
                                markAll={this.onChangeAllFilters('progressesList', true)}
                                clearAll={this.onChangeAllFilters('progressesList', false)}
                                multiSelection
                                disabled={late}
                                nameToDisplay="Description"
                            />
                        </div>
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.group"
                                    defaultMessage="Groups"
                                />
                                <span className="tl-primary-color tl-required"> *</span>
                            </span>
                            <TLSelectList
                                hint={formatMessage(messages.groupHint)}
                                items={groupsList}
                                onSelect={this.onChangeFilters('groupsList')}
                                markAll={this.onChangeAllFilters('groupsList', true)}
                                clearAll={this.onChangeAllFilters('groupsList', false)}
                                nameToDisplay="ColumnName"
                            />
                        </div>
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.search"
                                    defaultMessage="Search"
                                />
                            </span>
                            <input
                                type="text"
                                className="tl-input-field"
                                placeholder={formatMessage(messages.searchPlaceholder)}
                                onChange={this.onChangeSearch}
                                value={searchField}
                            />
                        </div>
                        <div className="tl-flex-col">
                            <span className="tl-filter-name">
                                <FormattedMessage
                                    id="taskFilers.lateTasks"
                                    defaultMessage="Only late tasks"
                                />
                            </span>
                            <input
                                type="checkbox"
                                onChange={this.changeLate}
                                checked={late}
                            />
                        </div>
                    </div>
                    <div className="tl-flex-col tl-apply-search">
                        <button
                            className="tl-default-button"
                            onClick={this.applyFilters()}
                            disabled={!loadedTasks}
                        >
                            <FormattedMessage
                                id="taskFilers.applyButton"
                                defaultMessage="Apply"
                            />
                        </button>
                    </div>
                </div>
            </div>
        )
    }
}

TaskFilters.propTypes = {
    currentUser: PropTypes.object.isRequired,
    filtersActions: PropTypes.object.isRequired,
    groupedTasksActions: PropTypes.object.isRequired,
    validatorsList: PropTypes.array.isRequired,
    responsibilitiesList: PropTypes.array.isRequired,
    groupsList: PropTypes.array.isRequired,
    progressesList: PropTypes.array.isRequired,
    groupedTasksList: PropTypes.array.isRequired,
    loadedTasks: PropTypes.bool.isRequired,
    late: PropTypes.bool.isRequired,
    unassigned: PropTypes.bool.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        currentUser: state.currentUser,
        validatorsList: state.filters.validatorsList,
        responsibilitiesList: state.filters.responsibilitiesList,
        groupsList: state.filters.groupsList,
        progressesList: state.filters.progressesList,
        late: state.filters.late,
        searchField: state.filters.searchField,
        unassigned: state.filters.unassigned,
        loadedTasks: state.filters.loadedTasks,
        groupedTasksList: state.groupedTasksList
    }
}

function mapDispatchToProps(dispatch) {
    return {
        filtersActions: bindActionCreators(filtersActions, dispatch),
        groupedTasksActions: bindActionCreators(groupedTasksActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(TaskFilters))