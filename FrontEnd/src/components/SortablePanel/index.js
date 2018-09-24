import React, {Component} from 'react'
import PropTypes from 'prop-types'
import { SortableContainer, SortableElement, arrayMove, SortableHandle } from 'react-sortable-hoc'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import { intlShape, injectIntl } from 'react-intl'

import './SortablePanel.css';
import * as configActions from '../../actions/configActions'
import generalMessages from '../../i18n/generalMessages'
import SortableDragIcon from '../svgIcons/SortableDragIcon'
import CloseIcon from '../svgIcons/CloseIcon'

class SortablePanel extends Component {
    constructor(props) {
        super(props);

        this.onSortEnd = this.onSortEnd.bind(this);
        this.handleClickOutside = this.handleClickOutside.bind(this);
    }

    componentDidMount() {
        document.addEventListener('mousedown', this.handleClickOutside);
    }

    componentWillUnmount() {
        document.removeEventListener('mousedown', this.handleClickOutside);
    }

    handleClickOutside(event) {
        if (this.menuNode && !this.menuNode.contains(event.target)) {
            this.props.configActions.toggleSortable(false)
        }
    }

    onSortEnd({oldIndex, newIndex}) {
        const {configActions, tableCols} = this.props;
        configActions.onSortEnd(arrayMove(tableCols, oldIndex, newIndex));
    }

    changeDisplayingColumn(column) {
        return () => {
            let {configActions, tableCols} = this.props;

            tableCols = tableCols.map(item => (
                item.id === column.id
                    ? {...item, visible: !item.visible}
                    : item
            ));

            configActions.onSortEnd(tableCols);
        }
    }

    togglePanel(isOpen) {
        return () => {
            this.props.configActions.toggleSortable(isOpen);
        }
    }

    render() {
        const {tableCols, isOpenSortable, intl: {formatMessage}} = this.props;

        const DragHandleElement = SortableHandle(() => <SortableDragIcon width={20} height={20} />);

        const SortableItem = SortableElement(({value}) =>
            <li className="tl-sortable-row">
                <label>
                    <input
                        type="checkbox"
                        onChange={this.changeDisplayingColumn(value)}
                        checked={value.visible}
                    />
                    {formatMessage(generalMessages[value.name])}
                </label>
                <DragHandleElement />
            </li>
        );

        const SortableList = SortableContainer(({items}) => {
            return (
                <ul>
                    {items.map((item, index) => (
                        <SortableItem
                            key={`item-${index}`}
                            index={index}
                            value={item}
                        />
                    ))}
                </ul>
            );
        });

        return (
            <div
                className={`tl-sortable-container ${isOpenSortable ? 'open' : ''}`}
                ref={menuNode => this.menuNode = menuNode}
            >
                <div className="tl-sortable-close">
                    <button onClick={this.togglePanel(!isOpenSortable)}>
                        <CloseIcon width={16} height={16} />
                    </button>
                </div>

                <SortableList
                    items={tableCols}
                    onSortEnd={this.onSortEnd}
                    useDragHandle={true}
                />
            </div>
        );
    }
}

SortablePanel.propTypes = {
    configActions: PropTypes.object.isRequired,
    tableCols: PropTypes.array.isRequired,
    isOpenSortable: PropTypes.bool.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        tableCols: state.config.tableCols,
        isOpenSortable: state.config.isOpenSortable
    }
}

function mapDispatchToProps(dispatch) {
    return {
        configActions: bindActionCreators(configActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(SortablePanel))