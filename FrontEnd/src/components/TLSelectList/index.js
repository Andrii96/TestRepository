// Multi select Component used in filters
import React, {Component} from 'react'
import PropTypes from 'prop-types'
import { intlShape, injectIntl, defineMessages, FormattedMessage } from 'react-intl'

import './TLSelectList.css';
import OkIcon from '../svgIcons/OkIcon'
import CloseIcon from '../svgIcons/CloseIcon'
import CancelSelectIcon from '../svgIcons/CancelSelectIcon'

const messages = defineMessages({
    unassigned: {
        id: 'selectList.unassigned',
        defaultMessage: 'Unassigned'
    },
    selectedCount: {
        id: 'selectList.selectCount',
        defaultMessage: '{selected} of {total}'
    }
});

class TLSelectList extends Component {
    constructor(props) {
        super(props);

        this.state = {
            openMenu: false
        };

        this.toggleMenu = this.toggleMenu.bind(this);
        this.handleClickOutside = this.handleClickOutside.bind(this);
        this.changeUnassigned = this.changeUnassigned.bind(this);
        this.markAll = this.markAll.bind(this);
        this.clearAll = this.clearAll.bind(this);
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

    changeUnassigned() {
        this.props.unassignedFunc();
    }

    markAll() {
        const {markAll, unassignedItem, unassignedFunc} = this.props;
        if (unassignedItem) unassignedFunc(true);
        markAll();
    }

    clearAll() {
        const {clearAll, unassignedItem, unassignedFunc} = this.props;
        if (unassignedItem) unassignedFunc(false);
        clearAll();
    }

    render() {
        const {openMenu} = this.state;
        const {multiSelection, nameToDisplay, unassignedItem, unassignedValue, intl: {formatMessage}} = this.props;
        let {items} = this.props;

        const selectedItems = items.filter(i => i.checked);

        return (
            <div className="tl-select-list">
                <button
                    className="tl-multiselect-btn"
                    onClick={this.toggleMenu}
                    disabled={this.props.disabled}
                >
                    { (selectedItems.length || unassignedValue)
                        ? ( (selectedItems.length + (unassignedValue ? 1 : 0) ) === 1)
                            ? selectedItems[0] ? selectedItems[0][nameToDisplay] : formatMessage(messages.unassigned)
                            : formatMessage(messages.selectedCount, {
                                selected: unassignedValue ? selectedItems.length + 1 : selectedItems.length,
                                total: unassignedItem ? items.length + 1 : items.length
                            })
                        : this.props.hint
                    }
                </button>
                {openMenu && <div className="tl-select-menu" ref={menuNode => {this.menuNode = menuNode;}}>
                    <div className="tl-select-menu-header">
                        <div className="tl-flex-row">
                            <div className="tl-flex-col">
                                {multiSelection && <button onClick={this.markAll}>
                                    <OkIcon width={16} height={16} />
                                    <FormattedMessage
                                        id="selectList.selectAll"
                                        defaultMessage="All"
                                    />
                                </button>}
                                {multiSelection && <button onClick={this.clearAll}>
                                    <CloseIcon width={16} height={16} />
                                    <FormattedMessage
                                        id="selectList.selectNone"
                                        defaultMessage="none"
                                    />
                                </button>}
                            </div>
                            <div className="tl-flex-col tl-close-multiselect">
                                <button
                                    onClick={this.toggleMenu}
                                >
                                    <CancelSelectIcon width={16} height={16} />
                                </button>
                            </div>
                        </div>

                    </div>
                    <ul>
                        {unassignedItem && <li>
                            <label>
                                <input
                                    type="checkbox"
                                    onChange={this.changeUnassigned}
                                    checked={unassignedValue}
                                />
                                <span>{formatMessage(messages.unassigned)}</span>
                            </label>
                        </li>}
                        {items.map((item, index) => (
                            <li key={index}>
                                <label>
                                    <input
                                        type="checkbox"
                                        onChange={this.changeSelected(index)}
                                        checked={item.checked ? item.checked : false}
                                    />
                                    <span>{item[nameToDisplay]}</span>
                                </label>
                            </li>
                        ))}
                        {!items[0] && <div className="tl-no-items">
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

TLSelectList.propTypes = {
    hint: PropTypes.string.isRequired,
    items: PropTypes.array.isRequired,
    onSelect: PropTypes.func.isRequired,
    markAll: PropTypes.func.isRequired,
    clearAll: PropTypes.func.isRequired,
    nameToDisplay: PropTypes.string.isRequired,
    intl: intlShape.isRequired
};

export default injectIntl(TLSelectList);