import React from 'react'
import PropTypes from 'prop-types'

const BookmarkIcon = ({
    width,
    height
}) => (
    <svg
        fill="#c50008"
        height={height}
        viewBox="0 0 24 24"
        width={width}
        xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M17 3H7c-1.1 0-1.99.9-1.99 2L5 21l7-3 7 3V5c0-1.1-.9-2-2-2zm0 15l-5-2.18L7 18V5h10v13z"/>
        <path d="M0 0h24v24H0z" fill="none"/>
    </svg>
);

BookmarkIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired
};


export default BookmarkIcon