import React, {Component, Fragment} from 'react'
import PropTypes from 'prop-types'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'

import './TLReassignSelectList.css';

const messages = defineMessages({
    unassigned: {
        id: 'selectReassignList.unassigned',
        defaultMessage: 'Unassigned'
    }
});

class TLReassignSelectList extends Component {
    constructor(props) {
        super(props);

        this.state = {
            openMenu: false
        };

        this.toggleMenu = this.toggleMenu.bind(this);
        this.handleClickOutside = this.handleClickOutside.bind(this);
        this.onSelectAssignee = this.onSelectAssignee.bind(this);
    }

    componentDidMount() {
        document.addEventListener('mousedown', this.handleClickOutside);
    }

    componentWillUnmount() {
        document.removeEventListener('mousedown', this.handleClickOutside);
    }

    handleClickOutside(event) {
        if (this.menuNode && !this.menuNode.contains(event.target)) {
            this.setState({openMenu: false})
        }
    }

    toggleMenu() {
        this.setState({openMenu: !this.state.openMenu})
    }

    changeSelected(selectedIndex) {
        return () => {
            let {onSelect, items, multiSelection} = this.props;

            if (multiSelection) {
                items = items.map((item, index) => (
                    index === selectedIndex
                        ? {...item, checked: !item.checked}
                        : item
                ));
            } else {
                items = items.map((item, index) => (
                    {...item, checked: index === selectedIndex}
                ));
                this.setState({openMenu: false});
            }

            onSelect(items);
        }
    }


    onSelectAssignee(id, newAssigneeName) {
        return () => {
            this.props.onSelectAssignee(id);
            this.setState({openMenu: false, newAssigneeName});
        }
    }


    render() {
        const {openMenu, newAssigneeName} = this.state;
        const {intl: {formatMessage}, assigneeGroups, currentAssigneeId} = this.props;

        let selectedAssignee = formatMessage(messages.unassigned);

        assigneeGroups.forEach(group => {
            group.Validators && group.Validators.forEach(validator => {
                if(validator.ContactId === currentAssigneeId)
                    selectedAssignee = validator.Name
            })
        });

        return (
            <div className="tl-select-list" style={{marginLeft: 5}}>
                <button
                    className="tl-multiselect-btn"
                    onClick={this.toggleMenu}
                    disabled={this.props.disabled}
                >
                    {newAssigneeName || selectedAssignee}
                </button>
                {openMenu && <div className="tl-select-menu tl-reassign-menu" ref={menuNode => {this.menuNode = menuNode}}>
                    <ul>
                        <li
                            className={!currentAssigneeId ? 'tl-active-assignee' : ''}
                            onClick={currentAssigneeId ? this.onSelectAssignee(null, formatMessage(messages.unassigned)) : null}
                        >
                            <label>
                                <span>{formatMessage(messages.unassigned)}</span>
                            </label>
                        </li>
                        {assigneeGroups.map((group, index) => (
                            <Fragment key={index}>
                                <li className="tl-reassign-menu-lang"><span>{group.LanguagePair}</span></li>
                                <li><hr/></li>
                                {group.Validators && group.Validators.map( (validator, index) => (
                                    <li key={index} className={currentAssigneeId === validator.ContactId ? 'tl-active-assignee' : ''}>
                                        <label onClick={currentAssigneeId !== validator.ContactId ? this.onSelectAssignee(validator.ContactId, validator.Name) : null}>
                                            <span>{validator.Name} [{validator.Priority}]</span>
                                        </label>
                                    </li>
                                ))}
                            </Fragment>
                        ))}
                        {!assigneeGroups[0] && <div className="tl-no-items">
                            <FormattedMessage
                                id="selectList.noItems"
                                defaultMessage="No results"
                            />
                        </div>}
                    </ul>

                </div>}
            </div>
        );
    }
}

TLReassignSelectList.propTypes = {
    assigneeGroups: PropTypes.array.isRequired,
    onSelectAssignee: PropTypes.func.isRequired,
    selectedUserId: PropTypes.number.isRequired,
    intl: intlShape.isRequired
};

export default injectIntl(TLReassignSelectList);