import React from 'react'
import PropTypes from 'prop-types'

const SortableDragIcon = ({
    width,
    height
}) => (
    <svg
        className="tl-drag-icon"
        fill="#000000"
        height={height}
        viewBox="0 0 24 24"
        width={width}
        xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M16 17.01V10h-2v7.01h-3L15 21l4-3.99h-3zM9 3L5 6.99h3V14h2V6.99h3L9 3z"/>
        <path d="M0 0h24v24H0z" fill="none"/>
    </svg>
);

SortableDragIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired
};


export default SortableDragIcon