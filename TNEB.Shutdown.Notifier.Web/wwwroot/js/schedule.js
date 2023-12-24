$(function () {
    const ps = new PerfectScrollbar("#schedule");

    $(document).on("resize", () => {
        ps.update();
    });
})