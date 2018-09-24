import React from 'react'
import PropTypes from 'prop-types'

const SortableIcon = ({
    width,
    height,
    onIconClick
}) => (
    <svg
        fill="#fff"
        height={height}
        width={width}
        viewBox="0 0 24 24"
        xmlns="http://www.w3.org/2000/svg"
        onClick={onIconClick}
    >
        <path
            d="M0 0h24v24H0z"
            fill="none"
        />
        <path
            d="M20 2H4c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zM8 20H4v-4h4v4zm0-6H4v-4h4v4zm0-6H4V4h4v4zm6 12h-4v-4h4v4zm0-6h-4v-4h4v4zm0-6h-4V4h4v4zm6 12h-4v-4h4v4zm0-6h-4v-4h4v4zm0-6h-4V4h4v4z"
        />
    </svg>
);

SortableIcon.propTypes = {
  width: PropTypes.number.isRequired,
  height: PropTypes.number.isRequired,
  onIconClick: PropTypes.func.isRequired
};


export default SortableIcon